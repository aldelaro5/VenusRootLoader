using System.Runtime.InteropServices;
using System.Text;

namespace VenusRootLoader.Bootstrap;

internal static class UnityPlayerLogsMirroring
{
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int WriteFileFn(nint hFile, nint lpBuffer, int nNumberOfBytesToWrite,
        ref int lpNumberOfBytesWritten, nint lpOverlapped);

    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    private delegate nint CreateFileWFn(string lpFileName,
        uint dwDesiredAccess,
        int dwShareMode,
        nint lpSecurityAttributes,
        int dwCreationDisposition,
        int dwFlagsAndAttributes,
        nint hTemplateFile);

    private static readonly CreateFileWFn HookCreateFileWDelegate = HookCreateFileW;
    private static readonly WriteFileFn HookWriteFileDelegate = HookWriteFile;
    
    private static bool _foundPlayerLogsHandle;
    private static nint _logHandle;
    
    private static readonly StringBuilder LogBuffer = new(2048);

    internal static void SetupPlayerLogMirroring()
    {
        PltHook.InstallHook(Entry.PlayerFileName, "CreateFileW", Marshal.GetFunctionPointerForDelegate(HookCreateFileWDelegate));
        PltHook.InstallHook(Entry.PlayerFileName, "WriteFile", Marshal.GetFunctionPointerForDelegate(HookWriteFileDelegate));
    }

    private static IntPtr HookCreateFileW(
        string lpfilename,
        uint dwdesiredaccess,
        int dwsharemode,
        IntPtr lpsecurityattributes,
        int dwcreationdisposition,
        int dwflagsandattributes,
        nint htemplatefile)
    {
        if (_foundPlayerLogsHandle)
        {
            return WindowsNative.CreateFileW(lpfilename, dwdesiredaccess, dwsharemode, lpsecurityattributes,
                dwcreationdisposition, dwflagsandattributes, htemplatefile);
        }

        if (lpfilename.EndsWith("Player.log") || lpfilename.EndsWith("output_log.txt"))
        {
            _logHandle = WindowsNative.CreateFileW(lpfilename, dwdesiredaccess, dwsharemode, lpsecurityattributes,
                dwcreationdisposition, dwflagsandattributes, htemplatefile);
            Console.WriteLine($"Found player logs file with handle 0x{_logHandle:X} at: {lpfilename}");

            _foundPlayerLogsHandle = true;
            return _logHandle;
        }
        return WindowsNative.CreateFileW(lpfilename, dwdesiredaccess, dwsharemode, lpsecurityattributes,
            dwcreationdisposition, dwflagsandattributes, htemplatefile);
    }

    private static int HookWriteFile(nint hFile, nint lpBuffer, int nNumberOfBytesToWrite,
                                    ref int lpNumberOfBytesWritten, nint lpOverlapped)
    {
        bool writeToPlayerLog = _foundPlayerLogsHandle && hFile == _logHandle;
        bool writeToStandardHandles = hFile == WindowsConsole.OutputHandle || hFile == WindowsConsole.ErrorHandle;
        if (writeToPlayerLog || writeToStandardHandles)
        {
            string log = Marshal.PtrToStringUTF8(lpBuffer, nNumberOfBytesToWrite);
            LogBuffer.Append(log);
            if (LogBuffer[^1] == '\n')
            {
                LogBuffer.Remove(LogBuffer.Length - 1, 1);
                Console.WriteLine(LogBuffer.ToString());
                LogBuffer.Clear();
            }
            if (writeToStandardHandles)
                return 1;
        }
        return WindowsNative.WriteFile(hFile, lpBuffer, nNumberOfBytesToWrite, ref lpNumberOfBytesWritten, lpOverlapped);
    }
}