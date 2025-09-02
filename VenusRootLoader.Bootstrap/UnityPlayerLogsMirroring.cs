using System.Drawing;
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
    private nint _outputHandle;
    private nint _errorHandle;

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int WriteFileFn(
        nint hFile,
        nint lpBuffer,
        int nNumberOfBytesToWrite,
        ref int lpNumberOfBytesWritten,
        nint lpOverlapped);
    private static WriteFileFn _hookWriteFileDelegate = null!;

    private nint _playerLogHandle = nint.Zero;
    private readonly StringBuilder _logBuffer = new(2048);

    private readonly PltHook _pltHook;
    private readonly ILogger _logger;
    private readonly CreateFileWSharedHooker _createFileWSharedHooker;

    public UnityPlayerLogsMirroring(ILoggerFactory loggerFactory, PltHook pltHook, CreateFileWSharedHooker createFileWSharedHooker)
    {
        _pltHook = pltHook;
        _createFileWSharedHooker = createFileWSharedHooker;
        _hookWriteFileDelegate = HookWriteFile;
        _logger = loggerFactory.CreateLogger("UNITY", Color.Aqua);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _outputHandle = WindowsNative.GetStdHandle(WindowsNative.StdOutputHandle);
        _errorHandle = WindowsNative.GetStdHandle(WindowsNative.StdErrorHandle);

        _pltHook.InstallHook(Entry.UnityPlayerDllFileName, "WriteFile", Marshal.GetFunctionPointerForDelegate(_hookWriteFileDelegate));
        _createFileWSharedHooker.RegisterHook(IsUnityPlayerLogFilename, HookFileHandle);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private bool IsUnityPlayerLogFilename(string lpFilename)
    {
        return lpFilename.EndsWith("Player.log") || lpFilename.EndsWith("output_log.txt");
    }

    private bool HookFileHandle(out nint originalHandle, string lpFilename, uint dwDesiredAccess, int dwShareMode, nint lpSecurityAttributes, int dwCreationDisposition, int dwFlagsAndAttributes, nint hTemplateFile)
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
        var writeToStandardHandles = hFile == _outputHandle || hFile == _errorHandle;
        if (!writeToPlayerLog && !writeToStandardHandles)
            return WindowsNative.WriteFile(hFile, lpBuffer, nNumberOfBytesToWrite, ref lpNumberOfBytesWritten, lpOverlapped);

        string log = Marshal.PtrToStringUTF8(lpBuffer, nNumberOfBytesToWrite);
        _logBuffer.Append(log);

        // Unity sometimes does multiline logs in one write.
        // For them to render correctly, we need to write each line one by one
        if (_logBuffer[^1] == '\n')
        {
            _logBuffer.Remove(_logBuffer.Length - 1, 1);
            _logger.LogTrace(_logBuffer.ToString());
            _logBuffer.Clear();
        }

        if (writeToStandardHandles)
            return 1;

        return WindowsNative.WriteFile(hFile, lpBuffer, nNumberOfBytesToWrite, ref lpNumberOfBytesWritten, lpOverlapped);
    }
}