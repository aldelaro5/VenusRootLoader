using HarmonyLib;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace VenusRootLoader.Preloader;

internal class GameLoadEntrypointInitializer : IHostedService
{
    private const string GameLoadHookAssemblyName = "UnityEngine.CoreModule";
    private const string GameLoadHookTypeName = "UnityEngine.SceneManagement.SceneManager";
    private const string GameLoadHookMethodName = "Internal_ActiveSceneChanged";

    private static readonly Harmony Harmony = new("VenusRootLoader");
    private static bool _monoCoreStartEntrypointAlreadyCalled;

    private readonly ILogger<GameLoadEntrypointInitializer> _logger;

    public GameLoadEntrypointInitializer(ILogger<GameLoadEntrypointInitializer> logger)
    {
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoad;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    // This hook allows to have a suitable GameLoad entrypoint without actually referencing the Unity assemblies
    private void OnAssemblyLoad(object sender, AssemblyLoadEventArgs args)
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
            _logger.LogInformation("Hooked into {methodDescription}", original.FullDescription());
        }
        catch (Exception e)
        {
            _logger.LogError(
                "Unexpected error occured when trying to hook into {AssemblyName}: {Exception}",
                assemblyName,
                e);
        }
    }

    private static bool Entrypoint()
    {
        if (_monoCoreStartEntrypointAlreadyCalled)
            return true;
        _monoCoreStartEntrypointAlreadyCalled = true;

        Assembly.Load("VenusRootLoader")
            .GetType("VenusRootLoader.Entry")
            .GetMethod("Main", AccessTools.all)!
            .Invoke(null, []);
        return true;
    }
}