using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace VenusRootLoader.Bootstrap;

/// <summary>
/// This class contains all the machinery needed to fully capture and mirror stdout, stderr and Unity's player logs
/// into our logs
/// </summary>
internal class UnityPlayerLogsMirroring : IHostedService
{
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int WriteFileFn(
        nint hFile,
        nint lpBuffer,
        int nNumberOfBytesToWrite,
        ref int lpNumberOfBytesWritten,
        nint lpOverlapped);
    private static WriteFileFn _hookWriteFileDelegate = null!;

    private static nint _playerLogHandle = nint.Zero;
    private static readonly StringBuilder LogBuffer = new(2048);
    private readonly ILogger<UnityPlayerLogsMirroring> _logger;

    public UnityPlayerLogsMirroring(ILogger<UnityPlayerLogsMirroring> logger)
    {
        _hookWriteFileDelegate = HookWriteFile;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("UnityPlayerLogsMirroring started");
        PltHook.InstallHook(Entry.UnityPlayerDllFileName, "WriteFile", Marshal.GetFunctionPointerForDelegate(_hookWriteFileDelegate));
        FileHandleHook.RegisterHook(IsUnityPlayerLogFilename, HookFileHandle);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static bool IsUnityPlayerLogFilename(string lpFilename)
    {
        return lpFilename.EndsWith("Player.log") || lpFilename.EndsWith("output_log.txt");
    }

    private static bool HookFileHandle(out nint originalHandle, string lpFilename, uint dwDesiredAccess, int dwShareMode, nint lpSecurityAttributes, int dwCreationDisposition, int dwFlagsAndAttributes, nint hTemplateFile)
    {
        originalHandle = WindowsNative.CreateFileW(lpFilename, dwDesiredAccess, dwShareMode, lpSecurityAttributes,
            dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
        _playerLogHandle = originalHandle;
        return false;
    }

    // This hook is what collects every stdout, stderr or player logs done by Unity and writes them to our logs
    private int HookWriteFile(
        nint hFile,
        nint lpBuffer,
        int nNumberOfBytesToWrite,
        ref int lpNumberOfBytesWritten,
        nint lpOverlapped)
    {
        var writeToPlayerLog = _playerLogHandle == hFile;
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
            _logger.LogTrace(LogBuffer.ToString());
            LogBuffer.Clear();
        }

        if (writeToStandardHandles)
            return 1;

        return WindowsNative.WriteFile(hFile, lpBuffer, nNumberOfBytesToWrite, ref lpNumberOfBytesWritten, lpOverlapped);
    }
}