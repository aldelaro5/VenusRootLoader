using Microsoft.Extensions.DependencyInjection;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using VenusRootLoader.BudLoading;

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
            IServiceProvider host = Startup.BuildServiceProvider(
                _gameExecutionContext,
                new() { BootstrapLog = _bootstrapLog });

            AppDomainEventsHandler appDomainEventsHandler = host.GetRequiredService<AppDomainEventsHandler>();
            appDomainEventsHandler.InstallHandlers();
            BudLoader loader = host.GetRequiredService<BudLoader>();
            loader.LoadAllBuds();
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