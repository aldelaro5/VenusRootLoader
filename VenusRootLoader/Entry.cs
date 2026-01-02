using Microsoft.Extensions.DependencyInjection;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.BaseGameCollector;
using VenusRootLoader.BudLoading;
using VenusRootLoader.Logging;
using VenusRootLoader.Patching.Resources.Sprite;
using VenusRootLoader.Patching.Resources.TextAsset;

namespace VenusRootLoader;

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global
internal static class Entry
{
    private const string BaseGameId = "BaseGame";

    private static GameExecutionContext? _gameExecutionContext;
    private static BootstrapFunctions.BootstrapLogFn _bootstrapLog = null!;
    private static string _basePath = null!;

    internal static void Main(nint bootstrapLogFunctionPtr, nint gameExecutionContextPtr, nint basePathPtr)
    {
        try
        {
            _bootstrapLog =
                Marshal.GetDelegateForFunctionPointer<BootstrapFunctions.BootstrapLogFn>(bootstrapLogFunctionPtr);
            _gameExecutionContext = Marshal.PtrToStructure<GameExecutionContext>(gameExecutionContextPtr);
            _basePath = Marshal.PtrToStringUni(basePathPtr)!;
            IServiceProvider host = Startup.BuildServiceProvider(
                _basePath,
                _gameExecutionContext,
                new() { BootstrapLog = _bootstrapLog });

            AppDomainEventsHandler appDomainEventsHandler = host.GetRequiredService<AppDomainEventsHandler>();
            appDomainEventsHandler.InstallHandlers();

            UnityLogger unityLogger = host.GetRequiredService<UnityLogger>();
            unityLogger.InstallManagedUnityLogger();

            BaseGameDataCollector gameDataCollector = host.GetRequiredService<BaseGameDataCollector>();
            gameDataCollector.CollectAndRegisterBaseGameData(BaseGameId);

            // TODO: Find a way to not need to do this
            TextAssetPatcher<ItemLeaf, int> patcher = host.GetRequiredService<TextAssetPatcher<ItemLeaf, int>>();
            LocalizedTextAssetPatcher<ItemLeaf, int> localizedPatcher =
                host.GetRequiredService<LocalizedTextAssetPatcher<ItemLeaf, int>>();
            ItemAndMedalSpritePatcher spritePatcher = host.GetRequiredService<ItemAndMedalSpritePatcher>();
            
            BudLoader loader = host.GetRequiredService<BudLoader>();
            loader.LoadAllBuds();
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