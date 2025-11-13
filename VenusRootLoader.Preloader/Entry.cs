using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System.Reflection;
using System.Runtime.InteropServices;

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace VenusRootLoader.Preloader;

/// <summary>
/// The preloader's sole purpose is to be the bridge between the bootstrap and the loader.
/// This class implements a loader entrypoint by runtime patching a Unity method that is invoked
/// early, but late enough so that all UnityEngine and game assemblies are available for usage.
/// It is too early in this assembly to reference the loader assembly, any UnityEngine or game assemblies
/// so this entry is done via Reflection and an AssemblyLoad AppDomain event. The preloader is designed to leave as
/// few traces as possible once the loader entrypoint is setup.
/// </summary>
internal class Entry
{
    /// <summary>
    /// Copied from Microsoft.Extensions.Logging, it's only there to not have a reference to it at runtime
    /// </summary>
    internal enum LogLevel
    {
        Trace = 0,
        Debug = 1,
        Information = 2,
        Warning = 3,
        Error = 4,
        Critical = 5,
        None = 6,
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    internal delegate void BootstrapLogFn(string message, string category, LogLevel logLevel);

    // We want some minimal logging capabilities to monitor the booting so we use the log function directly.
    internal static BootstrapLogFn BootstrapLog = null!;

    internal static nint BootstrapLogFunctionPtr;
    internal static nint GameExecutionContextPtr;

    private static readonly string LogCategory = typeof(Entry).Namespace!;

    // This particular entrypoint occurs once the earliest after the engine is mostly initialized
    private const string GameLoadHookAssemblyName = "UnityEngine.CoreModule";
    private const string GameLoadHookTypeName = "UnityEngine.SceneManagement.SceneManager";
    private const string GameLoadHookMethodName = "Internal_ActiveSceneChanged";

    private static bool _monoCoreStartEntrypointAlreadyCalled;

    private static ILHook _loaderEntrypointHook = null!;

    // These assemblies (especially MonoMod.Core) are force loaded early because there is a known issue with
    // Unity Mono where if some assemblies are loaded too early, it can lead to crashes.
    // This list is similar to what BepInEx does and they are so core to the modloader that it's not a huge deal
    // to force load them.
    private static readonly List<string> ForceLoadAssemblies =
    [
        "Mono.Cecil.dll",
        "Mono.Cecil.Mdb.dll",
        "Mono.Cecil.Pdb.dll",
        "Mono.Cecil.Rocks.dll",
        "MonoMod.Core.dll",
        "MonoMod.Iced.dll",
        "MonoMod.ILHelpers.dll",
        "MonoMod.RuntimeDetour.dll",
        "MonoMod.Utils.dll",
        "MonoMod.Backports.dll",
        "0Harmony.dll"
    ];

    /// <summary>
    /// This is the entry method called by the bootstrap. It receives 2 parameters that are meant to be forwarded to the loader.
    /// </summary>
    /// <param name="bootstrapLogFunctionPtr">A raw pointer to a <see cref="BootstrapLogFn"/> function for logging</param>
    /// <param name="gameExecutionContextPtr">A raw pointer to a struct containing information about the execution</param>
    internal static void Main(nint bootstrapLogFunctionPtr, nint gameExecutionContextPtr)
    {
        string pathAssemblies = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "VenusRootLoader");
        foreach (string ass in ForceLoadAssemblies)
            Assembly.LoadFrom(Path.Combine(pathAssemblies, ass));

        BootstrapLogFunctionPtr = bootstrapLogFunctionPtr;
        BootstrapLog = Marshal.GetDelegateForFunctionPointer<BootstrapLogFn>(bootstrapLogFunctionPtr);
        GameExecutionContextPtr = gameExecutionContextPtr;

        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoad;
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        BootstrapLog($"Unhandled exception: {e.ExceptionObject}", LogCategory, LogLevel.Error);
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

            Type sceneManagerType = assembly.GetType(GameLoadHookTypeName, false);
            MethodInfo original = sceneManagerType.GetMethod(
                GameLoadHookMethodName,
                BindingFlags.Static | BindingFlags.NonPublic)!;
            MethodInfo entryPoint = typeof(Entry).GetMethod(
                nameof(Entrypoint),
                BindingFlags.Static | BindingFlags.NonPublic)!;

            _loaderEntrypointHook = new ILHook(
                original,
                il =>
                {
                    ILCursor cursor = new(il);
                    cursor.EmitCall(entryPoint);
                });
            BootstrapLog($"Hooked into {original.Name}", LogCategory, LogLevel.Information);
        }
        catch (Exception e)
        {
            BootstrapLog($"Exception when setting up the GameLoad entry: {e}", LogCategory, LogLevel.Error);
        }
    }

    // This is the final stage of the loader entrypoint. When this is called for the first time thanks to the ILHook,
    // the entrypoint is complete and the loader entry can be called via reflection
    private static void Entrypoint()
    {
        if (_monoCoreStartEntrypointAlreadyCalled)
            return;
        _monoCoreStartEntrypointAlreadyCalled = true;
        _loaderEntrypointHook.Dispose();

        MethodInfo entryMethod = Assembly.Load("VenusRootLoader")
            .GetType("VenusRootLoader.Entry")
            .GetMethod("Main", BindingFlags.Static | BindingFlags.NonPublic)!;
        AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;

        entryMethod.Invoke(null, [BootstrapLogFunctionPtr, GameExecutionContextPtr]);
    }
}