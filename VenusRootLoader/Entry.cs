using Microsoft.Extensions.Hosting;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace VenusRootLoader;

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global
internal static class Entry
{
    private static GameExecutionContext? _gameExecutionContext;
    private static BootstrapFunctions.BootstrapLogFn _bootstrapLog = null!;
    
    internal static void Main(nint bootstrapLogFunctionPtr, nint gameExecutionContextPtr)
    {
        try
        {
            _bootstrapLog =
                Marshal.GetDelegateForFunctionPointer<BootstrapFunctions.BootstrapLogFn>(bootstrapLogFunctionPtr);
            _gameExecutionContext = Marshal.PtrToStructure<GameExecutionContext>(gameExecutionContextPtr);
            IHost host = Startup.BuildHost(_gameExecutionContext, new() { BootstrapLog = _bootstrapLog });
            host.Start();
        }
        catch (Exception e)
        {
            _bootstrapLog(
                "An unhandled exception occured during the loader's entrypoint: " + e,
                $"{nameof(VenusRootLoader)}.{nameof(Entry)}",
                LogLevel.Critical);
        }
    }
}