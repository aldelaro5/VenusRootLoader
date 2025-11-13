using Microsoft.Extensions.Hosting;
using System.Runtime.InteropServices;

namespace VenusRootLoader;

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global
internal static class Entry
{
    // private static UnityExplorer.ExplorerStandalone _explorerInstance;
    internal static void Main(nint bootstrapLogFunctionPtr, nint gameExecutionContextPtr)
    {
        var relayLogFunction = Marshal.GetDelegateForFunctionPointer<BootstrapFunctions.BootstrapLogFn>(bootstrapLogFunctionPtr);
        var gameExecutionContext = Marshal.PtrToStructure<GameExecutionContext>(gameExecutionContextPtr);
        var host = Startup.BuildHost(gameExecutionContext, new() { BootstrapLog = relayLogFunction });
        host.Start();
    }
}