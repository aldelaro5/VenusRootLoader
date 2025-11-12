using HarmonyLib;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Runtime.InteropServices;

namespace VenusRootLoader.Preloader;

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global
internal class Entry
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    internal delegate void LogMsgFn(string message, string category, LogLevel logLevel);

    internal static LogMsgFn LogMsg = null!;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    internal delegate void FreeGameExecutionContextFn(nint gameExecutionContextPtr);

    internal static FreeGameExecutionContextFn FreeGameExecutionContext = null!;

    internal static nint RelayLogFunctionPtr;
    internal static nint GameExecutionContextPtr;

    private const string LogCategory = "VenusRootLoader.Preloader";
    private const string GameLoadHookAssemblyName = "UnityEngine.CoreModule";
    private const string GameLoadHookTypeName = "UnityEngine.SceneManagement.SceneManager";
    private const string GameLoadHookMethodName = "Internal_ActiveSceneChanged";

    private static bool _monoCoreStartEntrypointAlreadyCalled;

    private static readonly Harmony HarmonyInstance = new(LogCategory);

    private static readonly string[] CriticalAssemblies =
    [
        "Mono.Cecil.dll",
        "Mono.Cecil.Mdb.dll",
        "Mono.Cecil.Pdb.dll",
        "Mono.Cecil.Rocks.dll",
        "MonoMod.Core.dll",
        "MonoMod.Iced.dll",
        "MonoMod.ILHelpers.dll",
        "MonoMod.Utils.dll",
        "MonoMod.RuntimeDetour.dll",
        "MonoMod.Backports.dll",
        "0Harmony.dll"
    ];

    internal static void Main(nint relayLogFunctionPtr, nint gameExecutionContextPtr)
    {
        string pathAssemblies = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "VenusRootLoader");
        foreach (string criticalAssembly in CriticalAssemblies)
            Assembly.LoadFrom(Path.Combine(pathAssemblies, criticalAssembly));

        RelayLogFunctionPtr = relayLogFunctionPtr;
        LogMsg = Marshal.GetDelegateForFunctionPointer<LogMsgFn>(relayLogFunctionPtr);
        GameExecutionContextPtr = gameExecutionContextPtr;

        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoad;
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        LogMsg($"Unhandled exception: {e.ExceptionObject}", LogCategory, LogLevel.Error);
    }

    // This hook allows to have a suitable GameLoad entrypoint without actually referencing the Unity assemblies
    private static void OnAssemblyLoad(object sender, AssemblyLoadEventArgs args)
    {
        var assembly = args.LoadedAssembly;
        var assemblyName = assembly.GetName().Name;
        if (assemblyName is not GameLoadHookAssemblyName)
            return;

        try
        {
            AppDomain.CurrentDomain.AssemblyLoad -= OnAssemblyLoad;

            var sceneManagerType = assembly.GetType(GameLoadHookTypeName, false);
            var original = AccessTools.Method(sceneManagerType, GameLoadHookMethodName);
            var harmonyMethod = new HarmonyMethod(typeof(Entry), nameof(Entrypoint));
            HarmonyInstance.Patch(original, prefix: harmonyMethod);
            LogMsg($"Hooked into {original.FullDescription()}", LogCategory, LogLevel.Information);
        }
        catch (Exception e)
        {
            LogMsg($"Exception when setting up the GameLoad entry: {e}", LogCategory, LogLevel.Error);
        }
    }

    private static bool Entrypoint()
    {
        if (_monoCoreStartEntrypointAlreadyCalled)
            return true;
        _monoCoreStartEntrypointAlreadyCalled = true;

        MethodInfo entryMethod = Assembly.Load("VenusRootLoader")
            .GetType("VenusRootLoader.Entry")
            .Method("Main");
        AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;

        entryMethod.Invoke(null, [RelayLogFunctionPtr, GameExecutionContextPtr]);
        return true;
    }
}