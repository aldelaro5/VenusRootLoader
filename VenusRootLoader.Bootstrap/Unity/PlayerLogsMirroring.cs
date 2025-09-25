using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using Windows.Win32.Security;
using Windows.Win32.Storage.FileSystem;
using Windows.Win32.System.Console;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VenusRootLoader.Bootstrap.Settings;
using VenusRootLoader.Bootstrap.Shared;

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

    private readonly IWin32 _win32;
    private readonly IPltHooksManager _pltHooksManager;
    private readonly ILogger _logger;
    private readonly CreateFileWSharedHooker _createFileWSharedHooker;
    private readonly GameExecutionContext _gameExecutionContext;
    private readonly GameLifecycleEvents _gameLifecycleEvents;

    private readonly bool _disableMirroring;

    public unsafe PlayerLogsMirroring(
        ILoggerFactory loggerFactory,
        IPltHooksManager pltHooksManager,
        CreateFileWSharedHooker createFileWSharedHooker,
        GameExecutionContext gameExecutionContext,
        IOptions<LoggingSettings> loggingSettings,
        GameLifecycleEvents gameLifecycleEvents,
        IWin32 win32)
    {
        _pltHooksManager = pltHooksManager;
        _logger = loggerFactory.CreateLogger("UNITY");
        _createFileWSharedHooker = createFileWSharedHooker;
        _gameExecutionContext = gameExecutionContext;
        _gameLifecycleEvents = gameLifecycleEvents;
        _win32 = win32;
        _disableMirroring = !loggingSettings.Value.IncludeUnityLogs!.Value;

        _hookWriteFileDelegate = HookWriteFile;
        _gameLifecycleEvents.Subscribe(OnGameLifecycle);
    }

    private void OnGameLifecycle(object? sender, GameLifecycleEventArgs e)
    {
        if (e.LifeCycle != GameLifecycle.MonoInitialising)
            return;
        _createFileWSharedHooker.UnregisterHook(nameof(PlayerLogsMirroring));
    }

    public unsafe Task StartAsync(CancellationToken cancellationToken)
    {
        _outputHandle = _win32.GetStdHandle(STD_HANDLE.STD_OUTPUT_HANDLE);
        _errorHandle = _win32.GetStdHandle(STD_HANDLE.STD_ERROR_HANDLE);

        _pltHooksManager.InstallHook(_gameExecutionContext.UnityPlayerDllFileName, "WriteFile", _hookWriteFileDelegate);
        _createFileWSharedHooker.RegisterHook(nameof(PlayerLogsMirroring), IsUnityPlayerLogFilename, HookFileHandle);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private bool IsUnityPlayerLogFilename(string lpFilename) =>
        lpFilename.EndsWith("Player.log") || lpFilename.EndsWith("output_log.txt");

    private unsafe void HookFileHandle(out HANDLE originalHandle, PCWSTR lpFileName, uint dwDesiredAccess, FILE_SHARE_MODE dwShareMode, SECURITY_ATTRIBUTES* lpSecurityAttributes, FILE_CREATION_DISPOSITION dwCreationDisposition, FILE_FLAGS_AND_ATTRIBUTES dwFlagsAndAttributes, HANDLE hTemplateFile)
    {
        originalHandle = _win32.CreateFile(lpFileName, dwDesiredAccess, dwShareMode, new(lpSecurityAttributes),
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
            return _win32.WriteFile(hFile, new(lpBuffer), nNumberOfBytesToWrite, new(lpNumberOfBytesWritten), new(lpOverlapped));

        if (_disableMirroring)
        {
            if (writeToStandardHandles)
                return 1;
            return _win32.WriteFile(hFile, new(lpBuffer), nNumberOfBytesToWrite, new(lpNumberOfBytesWritten), new(lpOverlapped));
        }

        string log = Marshal.PtrToStringUTF8((nint)lpBuffer, (int)nNumberOfBytesToWrite);
        _logger.LogTrace(log.TrimEnd("\r\n").ToString());

        if (writeToStandardHandles)
            return 1;

        return _win32.WriteFile(hFile, new(lpBuffer), nNumberOfBytesToWrite, new(lpNumberOfBytesWritten), new(lpOverlapped));
    }
}