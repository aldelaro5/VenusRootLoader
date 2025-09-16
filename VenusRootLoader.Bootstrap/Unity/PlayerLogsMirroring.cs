using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Security;
using Windows.Win32.Storage.FileSystem;
using Windows.Win32.System.Console;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VenusRootLoader.Bootstrap.Settings;
using VenusRootLoader.Bootstrap.Shared;
using PltHook = VenusRootLoader.Bootstrap.Shared.PltHook;

namespace VenusRootLoader.Bootstrap.Unity;

/// <summary>
/// This class contains all the machinery needed to fully capture and mirror stdout, stderr and Unity's player logs
/// into our logs
/// </summary>
internal class PlayerLogsMirroring : IHostedService
{
    private nint _outputHandle;
    private nint _errorHandle;

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private unsafe delegate int WriteFileFn(
        HANDLE hFile,
        byte* lpBuffer,
        uint nNumberOfBytesToWrite,
        uint* lpNumberOfBytesWritten,
        NativeOverlapped* lpOverlapped);
    private static WriteFileFn _hookWriteFileDelegate = null!;

    private nint _playerLogHandle = nint.Zero;

    private readonly PltHook _pltHook;
    private readonly ILogger _logger;
    private readonly CreateFileWSharedHooker _createFileWSharedHooker;
    private readonly GameExecutionContext _gameExecutionContext;
    private readonly bool _disableMirroring;

    public unsafe PlayerLogsMirroring(
        ILoggerFactory loggerFactory,
        PltHook pltHook,
        CreateFileWSharedHooker createFileWSharedHooker,
        GameExecutionContext gameExecutionContext,
        IOptions<LoggingSettings> loggingSettings)
    {
        _pltHook = pltHook;
        _logger = loggerFactory.CreateLogger("UNITY");
        _createFileWSharedHooker = createFileWSharedHooker;
        _gameExecutionContext = gameExecutionContext;
        _disableMirroring = loggingSettings.Value.DisableUnityLogs!.Value;

        _hookWriteFileDelegate = HookWriteFile;
    }

    public unsafe Task StartAsync(CancellationToken cancellationToken)
    {
        _outputHandle = PInvoke.GetStdHandle(STD_HANDLE.STD_OUTPUT_HANDLE);
        _errorHandle = PInvoke.GetStdHandle(STD_HANDLE.STD_ERROR_HANDLE);

        _pltHook.InstallHook(_gameExecutionContext.UnityPlayerDllFileName, "WriteFile", Marshal.GetFunctionPointerForDelegate(_hookWriteFileDelegate));
        _createFileWSharedHooker.RegisterHook(nameof(PlayerLogsMirroring), IsUnityPlayerLogFilename, HookFileHandle);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private bool IsUnityPlayerLogFilename(string lpFilename) =>
        lpFilename.EndsWith("Player.log") || lpFilename.EndsWith("output_log.txt");

    private unsafe void HookFileHandle(out HANDLE originalHandle, PCWSTR lpFileName, uint dwDesiredAccess, FILE_SHARE_MODE dwShareMode, SECURITY_ATTRIBUTES* lpSecurityAttributes, FILE_CREATION_DISPOSITION dwCreationDisposition, FILE_FLAGS_AND_ATTRIBUTES dwFlagsAndAttributes, HANDLE hTemplateFile)
    {
        originalHandle = PInvoke.CreateFile(lpFileName, dwDesiredAccess, dwShareMode, lpSecurityAttributes,
            dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
        _playerLogHandle = originalHandle;
        _createFileWSharedHooker.UnregisterHook(nameof(PlayerLogsMirroring));
    }

    // This hook is what collects every stdout, stderr or player logs done by Unity and writes them to our logs
    private unsafe int HookWriteFile(
        HANDLE hFile,
        byte* lpBuffer,
        uint nNumberOfBytesToWrite,
        uint* lpNumberOfBytesWritten,
        NativeOverlapped* lpOverlapped)
    {
        var writeToPlayerLog = _playerLogHandle == hFile;
        var writeToStandardHandles = hFile == _outputHandle || hFile == _errorHandle;
        if (!writeToPlayerLog && !writeToStandardHandles)
            return PInvoke.WriteFile(hFile, lpBuffer, nNumberOfBytesToWrite, lpNumberOfBytesWritten, lpOverlapped);

        if (_disableMirroring)
        {
            if (writeToStandardHandles)
                return 1;
            return PInvoke.WriteFile(hFile, lpBuffer, nNumberOfBytesToWrite, lpNumberOfBytesWritten, lpOverlapped);
        }

        string log = Marshal.PtrToStringUTF8((nint)lpBuffer, (int)nNumberOfBytesToWrite);
        _logger.LogTrace(log.TrimEnd("\r\n").ToString());

        if (writeToStandardHandles)
            return 1;

        return PInvoke.WriteFile(hFile, lpBuffer, nNumberOfBytesToWrite, lpNumberOfBytesWritten, lpOverlapped);
    }
}