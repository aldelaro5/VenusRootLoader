using HarmonyLib;

namespace VenusRootLoader;

public static class GameLoadEntrypointInitializer
{
    private const string GameLoadHookAssemblyName = "UnityEngine.CoreModule";
    private const string GameLoadHookTypeName = "UnityEngine.SceneManagement.SceneManager";
    private const string GameLoadHookMethodName = "Internal_ActiveSceneChanged";

    private static readonly Harmony Harmony = new("VenusRootLoader");
    private static bool _monoCoreStartEntrypointAlreadyCalled;

    public static void Setup()
    {
        AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoad;
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
            var harmonyMethod = new HarmonyMethod(typeof(GameLoadEntrypointInitializer), nameof(Entrypoint));
            Harmony.Patch(original, prefix: harmonyMethod);
            Console.WriteLine($"Hooked into {original.FullDescription()}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Unexpected error occured when trying to hook into {assemblyName}: {e}");
        }
    }

    private static bool Entrypoint()
    {
        if (_monoCoreStartEntrypointAlreadyCalled)
            return true;
        _monoCoreStartEntrypointAlreadyCalled = true;

        GameLoadEntry.Main();
        return true;
    }
}