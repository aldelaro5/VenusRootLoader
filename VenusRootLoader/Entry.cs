using Microsoft.Extensions.Hosting;
using System.Reflection;
using System.Runtime.InteropServices;
using HarmonyLib;
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
            var host = Startup.BuildHost(_gameExecutionContext, new() { BootstrapLog = _bootstrapLog });
            host.Start();
            string modsPath = Path.Combine(_gameExecutionContext.GameDir, "Mods");
            Assembly.LoadFrom(Path.Combine(modsPath, "UnityExplorer.Standalone.MonoBleedingEdge.dll"));
            var type = AccessTools.TypeByName("ExplorerStandalone");
            var method = AccessTools.Method(type, "CreateInstance");
            method.Invoke(null, []);
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