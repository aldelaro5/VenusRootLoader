using System.Runtime.InteropServices;
using System.Text;

namespace VenusRootLoader.Bootstrap;

internal static class UnityPlayerLogsMirroring
{
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int WriteFileFn(nint hFile, nint lpBuffer, int nNumberOfBytesToWrite,
        ref int lpNumberOfBytesWritten, nint lpOverlapped);

    private static readonly WriteFileFn HookWriteFileDelegate = HookWriteFile;
    
    private static readonly StringBuilder LogBuffer = new(2048);

    internal static void SetupPlayerLogMirroring()
    {
        PltHook.InstallHook(Entry.PlayerFileName, "WriteFile", Marshal.GetFunctionPointerForDelegate(HookWriteFileDelegate));
    }

    private static int HookWriteFile(nint hFile, nint lpBuffer, int nNumberOfBytesToWrite,
                                    ref int lpNumberOfBytesWritten, nint lpOverlapped)
    {
        if (hFile != WindowsConsole.OutputHandle && hFile != WindowsConsole.ErrorHandle)
        {
            return WindowsNative.WriteFile(hFile, lpBuffer, nNumberOfBytesToWrite, ref lpNumberOfBytesWritten, lpOverlapped);
        }

        string log = Marshal.PtrToStringUTF8(lpBuffer, nNumberOfBytesToWrite);
        LogBuffer.Append(log);
        if (LogBuffer[^1] == '\n')
        {
            LogBuffer.Remove(LogBuffer.Length - 1, 1);
            Console.WriteLine(LogBuffer.ToString());
            LogBuffer.Clear();
        }

        return 1;
    }
}