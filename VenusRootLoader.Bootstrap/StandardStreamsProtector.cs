using System.Runtime.InteropServices;
using Microsoft.Extensions.Hosting;

namespace VenusRootLoader.Bootstrap;

internal class StandardStreamsProtector : IHostedService
{
    private nint _outputHandle;
    private nint _errorHandle;

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int CloseHandleFn(nint hObject);
    private static CloseHandleFn _hookCloseHandleDelegate = null!;

    private readonly PltHook _pltHook;

    public StandardStreamsProtector(PltHook pltHook)
    {
        _pltHook = pltHook;
        _hookCloseHandleDelegate = HookCloseHandle;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _outputHandle = WindowsNative.GetStdHandle(WindowsNative.StdOutputHandle);
        _errorHandle = WindowsNative.GetStdHandle(WindowsNative.StdErrorHandle);

        _pltHook.InstallHook(Entry.UnityPlayerDllFileName, "CloseHandle", Marshal.GetFunctionPointerForDelegate(_hookCloseHandleDelegate));
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    // Unity may attempt to close stdout and stderr in order to redirect their streams to their player logs.
    // Since we attempt to control all logging, we want to prevent this from happening which is what this hook is for
    private int HookCloseHandle(nint hObject)
    {
        if (hObject == _outputHandle || hObject == _errorHandle)
        {
            Console.WriteLine($"Prevented the CloseHandle of {(hObject == _outputHandle ? "stdout" : "stderr")}");
            return 1;
        }

        return WindowsNative.CloseHandle(hObject);
    }
}