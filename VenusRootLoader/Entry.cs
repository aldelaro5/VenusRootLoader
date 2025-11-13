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
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
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

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        _bootstrapLog($"Unhandled exception: {e.ExceptionObject}", nameof(Entry), LogLevel.Error);
    }

    private static Assembly? OnAssemblyResolve(object sender, ResolveEventArgs args)
    {
        if (_gameExecutionContext is null)
            return null;

        AssemblyName assemblyName = new(args.Name);
        string vrlPath = Path.Combine(_gameExecutionContext.GameDir, nameof(VenusRootLoader));
        if (File.Exists(Path.Combine(vrlPath, $"{assemblyName.Name}.dll")))
            return Assembly.LoadFrom(Path.Combine(vrlPath, $"{assemblyName.Name}.dll"));

        string modsPath = Path.Combine(_gameExecutionContext.GameDir, "Mods");
        if (File.Exists(Path.Combine(modsPath, $"{assemblyName.Name}.dll")))
            return Assembly.LoadFrom(Path.Combine(modsPath, $"{assemblyName.Name}.dll"));

        _bootstrapLog("Unable to find assembly " + args.Name, nameof(Entry), LogLevel.Error);
        return null;
    }
}