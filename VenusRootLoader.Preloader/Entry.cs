using Microsoft.Extensions.Hosting;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: InternalsVisibleTo("VenusRootLoader")]

namespace VenusRootLoader.Preloader;

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global
internal class Entry
{
    internal static void Main(nint relayLogFunctionPtr, nint gameExecutionContextPtr)
    {
        var relayLogFunction = Marshal.GetDelegateForFunctionPointer<BootstrapFunctions.LogMsgFn>(relayLogFunctionPtr);
        var gameExecutionContext = Marshal.PtrToStructure<GameExecutionContext>(gameExecutionContextPtr);
        var host = Startup.BuildHost(gameExecutionContext, new() { LogMsg = relayLogFunction });
        host.Start();
    }
}