using System.Runtime.InteropServices;
using System.Text;

namespace VenusRootLoader.Bootstrap;

/// <summary>
/// This class contains all the machinery needed to fully capture and mirror stdout, stderr and Unity's player logs
/// into our logs
/// </summary>
internal static class UnityPlayerLogsMirroring
{
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int WriteFileFn(
        nint hFile,
        nint lpBuffer,
        int nNumberOfBytesToWrite,
        ref int lpNumberOfBytesWritten,
        nint lpOverlapped);
    private static readonly WriteFileFn HookWriteFileDelegate = HookWriteFile;

    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    private delegate nint CreateFileWFn(string lpFileName,
        uint dwDesiredAccess,
        int dwShareMode,
        nint lpSecurityAttributes,
        int dwCreationDisposition,
        int dwFlagsAndAttributes,
        nint hTemplateFile);
    private static readonly CreateFileWFn HookCreateFileWDelegate = HookCreateFileW;

    private static readonly StringBuilder LogBuffer = new(2048);

    private static bool _foundPlayerLogsHandle;
    private static nint _logHandle;

    internal static void SetUp()
    {
        PltHook.InstallHook(Entry.UnityPlayerDllFileName, "WriteFile", Marshal.GetFunctionPointerForDelegate(HookWriteFileDelegate));
        PltHook.InstallHook(Entry.UnityPlayerDllFileName, "CreateFileW", Marshal.GetFunctionPointerForDelegate(HookCreateFileWDelegate));
    }

    private static bool IsUnityPlayerLogFilename(string lpFilename)
    {
        return lpFilename.EndsWith("Player.log") || lpFilename.EndsWith("output_log.txt");
    }

    // This hook is there to obtain the player log filename if Unity is trying to create / open it so we can collect its handle
    private static nint HookCreateFileW(
        string lpFilename,
        uint dwDesiredAccess,
        int dwShareMode,
        nint lpSecurityAttributes,
        int dwCreationDisposition,
        int dwFlagsAndAttributes,
        nint hTemplateFile)
    {
        var fileHandle = WindowsNative.CreateFileW(lpFilename, dwDesiredAccess, dwShareMode, lpSecurityAttributes,
            dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);

        if (_foundPlayerLogsHandle || !IsUnityPlayerLogFilename(lpFilename))
            return fileHandle;

        _logHandle = fileHandle;
        Console.WriteLine($"Found player logs file with handle 0x{_logHandle:X} at: {lpFilename}");

        _foundPlayerLogsHandle = true;
        return _logHandle;
    }

    // This hook is what collects every stdout, stderr or player logs done by Unity and writes them to our logs
    private static int HookWriteFile(
        nint hFile,
        nint lpBuffer,
        int nNumberOfBytesToWrite,
        ref int lpNumberOfBytesWritten,
        nint lpOverlapped)
    {
        var writeToPlayerLog = _foundPlayerLogsHandle && hFile == _logHandle;
        var writeToStandardHandles = hFile == WindowsConsole.OutputHandle || hFile == WindowsConsole.ErrorHandle;
        if (!writeToPlayerLog && !writeToStandardHandles)
            return WindowsNative.WriteFile(hFile, lpBuffer, nNumberOfBytesToWrite, ref lpNumberOfBytesWritten, lpOverlapped);

        string log = Marshal.PtrToStringUTF8(lpBuffer, nNumberOfBytesToWrite);
        LogBuffer.Append(log);
        
        // Unity sometimes does multiline logs in one write.
        // For them to render correctly, we need to write each line one by one
        if (LogBuffer[^1] == '\n')
        {
            LogBuffer.Remove(LogBuffer.Length - 1, 1);
            Console.WriteLine(LogBuffer.ToString());
            LogBuffer.Clear();
        }

        if (writeToStandardHandles)
            return 1;

        return WindowsNative.WriteFile(hFile, lpBuffer, nNumberOfBytesToWrite, ref lpNumberOfBytesWritten, lpOverlapped);
    }
}