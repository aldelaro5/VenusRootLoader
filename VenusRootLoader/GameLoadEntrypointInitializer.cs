using System.Reflection;
using HarmonyLib;

namespace VenusRootLoader;

public static class GameLoadEntrypointInitializer
{
    private static Harmony _harmony = new("VenusRootLoader");
    private static bool _monoCoreStartEntrypointAlreadyCalled;
    private static MethodInfo _monoCoreStartHookMethod;

    public static void Setup()
    {
        AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoad;
    }

    // First step of the ??? entrypoint: Harmony patch the main Unity assembly with a suitable hook
    private static void OnAssemblyLoad(object sender, AssemblyLoadEventArgs args)
    {
        const string sceneManagerTypeName = "UnityEngine.SceneManagement.SceneManager";
        const string displayTypeName = "UnityEngine.Display";

        var assembly = args.LoadedAssembly;
        var assemblyName = assembly.GetName().Name;

        if (assemblyName is not ("UnityEngine.CoreModule" or "UnityEngine"))
            return;

        try
        {
            AppDomain.CurrentDomain.AssemblyLoad -= OnAssemblyLoad;

            var sceneManagerType = assembly.GetType(sceneManagerTypeName, false);
            if (sceneManagerType != null)
            {
                _monoCoreStartHookMethod = sceneManagerType.GetMethod("Internal_ActiveSceneChanged",
                    BindingFlags.NonPublic | BindingFlags.Static);
                _harmony.Patch(_monoCoreStartHookMethod,
                    prefix: new HarmonyMethod(typeof(GameLoadEntrypointInitializer), nameof(Entrypoint)));
                Console.WriteLine($"Hooked into {_monoCoreStartHookMethod.FullDescription()}");
                return;
            }

            var displayType = assembly.GetType(displayTypeName, false);
            if (displayType != null)
            {
                _monoCoreStartHookMethod =
                    displayType.GetMethod("RecreateDisplayList", BindingFlags.NonPublic | BindingFlags.Static);
                _harmony.Patch(_monoCoreStartHookMethod,
                    postfix: new HarmonyMethod(typeof(GameLoadEntrypointInitializer), nameof(Entrypoint)));
                Console.WriteLine($"Hooked into {_monoCoreStartHookMethod.FullDescription()}");
                return;
            }

            Console.WriteLine(
                $"Couldn't find a suitable Core.Start entrypoint in the {assemblyName} assembly because " +
                $"{sceneManagerTypeName} or {displayTypeName} do not exist in the assembly");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Unexpected error occured when trying to hook into {assemblyName}: {e}");
        }
    }

    // Second step of the Mono Core.Start entrypoint: undo the Harmony patch and call the Core.Start method
    private static void Entrypoint()
    {
        if (_monoCoreStartEntrypointAlreadyCalled)
            return;

        _monoCoreStartEntrypointAlreadyCalled = true;
        try
        {
           _harmony.Unpatch(_monoCoreStartHookMethod, HarmonyPatchType.All, "VenusRootLoader");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Unexpected error when trying to unhook the Core.Start entrypoint: {e}");
        }

        GameLoadEntry.Main();
    }
}