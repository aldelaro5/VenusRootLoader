using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;
using VenusRootLoader.Bootstrap.Shared;
using Windows.Win32.Foundation;
using Windows.Win32.System.Console;

namespace VenusRootLoader.Bootstrap.Logging;

/// <summary>
/// This service makes sure Unity isn't closing stdout and stderr on us. This can happen because Unity may want to
/// redirect these streams to their own logs (it might even be possible for Unity to still use the console, but it can
/// still reset the streams to different handles!). This is achieved with a CloseHandle PltHook.
/// </summary>
internal sealed class StandardStreamsProtector : IHostedService
{
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate BOOL CloseHandleFn(HANDLE hObject);

    private static CloseHandleFn _hookCloseHandleDelegate = null!;

    private readonly IWin32 _win32;
    private readonly IPltHooksManager _pltHooksManager;
    private readonly GameExecutionContext _gameExecutionContext;
    private readonly ILogger _logger;
    private readonly IMonoInitLifeCycleEvents _monoInitLifeCycleEvents;

    private HANDLE _outputHandle;
    private HANDLE _errorHandle;

    public StandardStreamsProtector(
        ILogger<StandardStreamsProtector> logger,
        IPltHooksManager pltHooksManager,
        GameExecutionContext gameExecutionContext,
        IMonoInitLifeCycleEvents monoInitLifeCycleEvents,
        IWin32 win32)
    {
        _pltHooksManager = pltHooksManager;
        _logger = logger;
        _gameExecutionContext = gameExecutionContext;
        _monoInitLifeCycleEvents = monoInitLifeCycleEvents;
        _win32 = win32;
        _hookCloseHandleDelegate = HookCloseHandle;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _outputHandle = _win32.GetStdHandle(STD_HANDLE.STD_OUTPUT_HANDLE);
        _errorHandle = _win32.GetStdHandle(STD_HANDLE.STD_ERROR_HANDLE);

        _pltHooksManager.InstallHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            "CloseHandle",
            _hookCloseHandleDelegate);
        _monoInitLifeCycleEvents.Subscribe(OnGameLifecycle);
        return Task.CompletedTask;
    }

    // By this point, we know the streams are safe so we can unhook ourselves
    private void OnGameLifecycle(object? sender, EventArgs e)
    {
        _pltHooksManager.UninstallHook(_gameExecutionContext.UnityPlayerDllFileName, "CloseHandle");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private BOOL HookCloseHandle(HANDLE hObject)
    {
        if (!_win32.CompareObjectHandles(hObject, _outputHandle) && !_win32.CompareObjectHandles(hObject, _errorHandle))
            return _win32.CloseHandle(hObject);

        _logger.LogInformation(
            "Prevented the CloseHandle of {StreamName}",
            hObject == _outputHandle ? "stdout" : "stderr");
        return true;
    }
}