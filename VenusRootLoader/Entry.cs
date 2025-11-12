using Microsoft.Extensions.Hosting;
using System.Runtime.InteropServices;

namespace VenusRootLoader;

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global
internal static class Entry
{
    // private static UnityExplorer.ExplorerStandalone _explorerInstance;
    internal static void Main(nint relayLogFunctionPtr, nint gameExecutionContextPtr)
    {
        var relayLogFunction = Marshal.GetDelegateForFunctionPointer<BootstrapFunctions.LogMsgFn>(relayLogFunctionPtr);
        var gameExecutionContext = Marshal.PtrToStructure<GameExecutionContext>(gameExecutionContextPtr);
        var host = Startup.BuildHost(gameExecutionContext, new() { LogMsg = relayLogFunction });
        host.Start();
    }
}