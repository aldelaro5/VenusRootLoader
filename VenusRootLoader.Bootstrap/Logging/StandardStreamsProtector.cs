using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Console;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VenusRootLoader.Bootstrap.Shared;

namespace VenusRootLoader.Bootstrap.Logging;

internal class StandardStreamsProtector : IHostedService
{
    private nint _outputHandle;
    private nint _errorHandle;

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int CloseHandleFn(HANDLE hObject);
    private static CloseHandleFn _hookCloseHandleDelegate = null!;

    private readonly IPltHooksManager _pltHooksManager;
    private readonly GameExecutionContext _gameExecutionContext;
    private readonly ILogger _logger;
    private readonly GameLifecycleEvents _gameLifecycleEvents;

    public StandardStreamsProtector(
        ILogger<StandardStreamsProtector> logger,
        IPltHooksManager pltHooksManager,
        GameExecutionContext gameExecutionContext,
        GameLifecycleEvents gameLifecycleEvents)
    {
        _pltHooksManager = pltHooksManager;
        _logger = logger;
        _gameExecutionContext = gameExecutionContext;
        _gameLifecycleEvents = gameLifecycleEvents;
        _hookCloseHandleDelegate = HookCloseHandle;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _outputHandle = PInvoke.GetStdHandle(STD_HANDLE.STD_OUTPUT_HANDLE);
        _errorHandle = PInvoke.GetStdHandle(STD_HANDLE.STD_ERROR_HANDLE);

        _pltHooksManager.InstallHook(_gameExecutionContext.UnityPlayerDllFileName, "CloseHandle", Marshal.GetFunctionPointerForDelegate(_hookCloseHandleDelegate));
        _gameLifecycleEvents.Subscribe(OnGameLifecycle);
        return Task.CompletedTask;
    }

    private void OnGameLifecycle(object? sender, GameLifecycleEventArgs e)
    {
        if (e.LifeCycle != GameLifecycle.MonoInitialising)
            return;
        _pltHooksManager.UninstallHook(_gameExecutionContext.UnityPlayerDllFileName, "CloseHandle");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    // Unity may attempt to close stdout and stderr in order to redirect their streams to their player logs.
    // Since we attempt to control all logging, we want to prevent this from happening which is what this hook is for
    private int HookCloseHandle(HANDLE hObject)
    {
        if (hObject != _outputHandle && hObject != _errorHandle)
            return PInvoke.CloseHandle(hObject);

        _logger.LogInformation("Prevented the CloseHandle of {StreamName}", hObject == _outputHandle ? "stdout" : "stderr");
        return 1;
    }
}