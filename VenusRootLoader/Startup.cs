using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using UnityEngine;
using VenusRootLoader.Api;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.BaseGameCollector;
using VenusRootLoader.BudLoading;
using VenusRootLoader.Extensions;
using VenusRootLoader.Logging;
using VenusRootLoader.Patching;
using VenusRootLoader.Patching.Logic;
using VenusRootLoader.Patching.Resources;
using VenusRootLoader.Patching.Resources.Sprites;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Patching.Resources.TextAsset.Parsers;
using VenusRootLoader.Patching.Resources.TextAsset.Parsers.GlobalData;
using VenusRootLoader.Patching.Resources.TextAsset.Parsers.LocalisedData;
using VenusRootLoader.Patching.Resources.TextAsset.Parsers.OrderingData;
using VenusRootLoader.Registry;
using VenusRootLoader.Unity;

namespace VenusRootLoader;

internal static class Startup
{
    internal static IServiceProvider BuildServiceProvider(
        string basePath,
        GameExecutionContext gameExecutionContext,
        BootstrapFunctions bootstrapFunctions)
    {
        IServiceCollection services = new ServiceCollection();
        IConfigurationManager configurationManager = new ConfigurationManager();

        FileSystem fileSystem = new();
        string configPath = fileSystem.Path.Combine(basePath, "Config");
        configurationManager.AddJsonFile(fileSystem.Path.Combine(configPath, "config.jsonc"));

        services.AddSingleton(gameExecutionContext);
        services.AddSingleton(bootstrapFunctions);
        services.AddSingleton(
            new BudLoaderContext
            {
                BudsPath = fileSystem.Path.Combine(basePath, "Buds"),
                ConfigPath = configPath,
                LoaderPath = fileSystem.Path.Combine(gameExecutionContext.GameDir, nameof(VenusRootLoader)),
            });

        services.AddLogging(builder =>
        {
            builder.AddConfiguration(configurationManager.GetRequiredSection("Logging"));
            builder.Services.AddSingleton<ILoggerProvider, RelayLoggerProvider>();
        });

        services.AddSingleton<IFileSystem, FileSystem>();
        services.AddSingleton<IAppDomainEvents, AppDomainEvents>();
        services.AddSingleton<AppDomainEventsHandler>();
        services.AddSingleton<HarmonyLogger>();
        services.AddSingleton<IHarmonyTypePatcher, HarmonyTypePatcher>();
        services.AddSingleton<UnityLogger>();

        services.AddSingleton<EnumPatcher>();

        services.AddSingleton<ILeavesRegistry<ItemLeaf>, ItemsRegistry>();
        services.AddOrderedLeavesRegistry<MedalLeaf, MedalsRegistry>();
        services.AddOrderedLeavesRegistry<EnemyLeaf, EnemiesRegistry>();
        services.AddSingleton<ILeavesRegistry<RecipeLeaf>, RecipesRegistry>();
        services.AddSingleton<ILeavesRegistry<RecipeLibraryEntryLeaf>, RecipeLibraryEntriesRegistry>();
        services.AddSingleton<ILeavesRegistry<AreaLeaf>, AreasRegistry>();
        services.AddSingleton<ILeavesRegistry<FlagLeaf>, FlagsRegistry>();
        services.AddSingleton<ILeavesRegistry<FlagvarLeaf>, FlagvarsRegistry>();
        services.AddSingleton<ILeavesRegistry<FlagstringLeaf>, FlagstringsRegistry>();
        services.AddSingleton<ILeavesRegistry<CrystalBerryLeaf>, CrystalBerriesRegistry>();
        services.AddSingleton<ILeavesRegistry<CommonDialogueLeaf>, CommonDialoguesRegistry>();
        services.AddSingleton<ILeavesRegistry<MenuTextLeaf>, MenuTextsRegistry>();
        services.AddSingleton<ILeavesRegistry<PrizeMedalLeaf>, PrizeMedalsRegistry>();
        services.AddOrderedLeavesRegistry<DiscoveryLeaf, DiscoveriesRegistry>();
        services.AddOrderedLeavesRegistry<RecordLeaf, RecordsRegistry>();
        services.AddSingleton<ILeavesRegistry<TermacadePrizeLeaf>, TermacadePrizesRegistry>();
        services.AddSingleton<IRegistryResolver, RegistryResolver>();

        services.AddSingleton<ISpriteArrayPatcher, EnemyPortraitsSpriteArrayPatcher>(provider =>
            new(
                ["Items/EnemyPortraits"],
                provider.GetRequiredService<ILeavesRegistry<DiscoveryLeaf>>(),
                provider.GetRequiredService<ILeavesRegistry<EnemyLeaf>>(),
                provider.GetRequiredService<ILeavesRegistry<RecordLeaf>>()));

        services.AddTextAssetPatcher<ItemLeaf, ItemTextAssetParser>(["ItemData"]);
        services.AddLocalizedTextAssetPatcher<ItemLeaf, ItemLocalizedTextAssetParser>(["Items"]);

        services.AddTextAssetPatcher<MedalLeaf, MedalTextAssetParser>(["BadgeData"]);
        services.AddOrderingTextAssetPatcher<MedalLeaf, MedalOrderingTextAssetParser>("BadgeOrder");
        services.AddLocalizedTextAssetPatcher<MedalLeaf, MedalLocalizedTextAssetParser>(["BadgeName"]);

        services.AddLocalizedTextAssetPatcher<CrystalBerryLeaf, CrystalBerryLocalizedTextAssetParser>(
            ["FortuneTeller0"]);
        
        services.AddLocalizedTextAssetPatcher<CommonDialogueLeaf, CommonDialoguelLocalizedTextAssetParser>(
            ["CommonDialogue"],
            r => r.LeavesByNamedIds.Values.OrderBy(l => l.InternalGameIndex));

        services.AddLocalizedTextAssetPatcher<MenuTextLeaf, MenuTextLocalizedTextAssetParser>(["MenuText"]);
        
        services.AddOrderingTextAssetPatcher<DiscoveryLeaf, DiscoveryOrderingTextAssetParser>("DiscoveryOrder");
        services.AddLocalizedTextAssetPatcher<DiscoveryLeaf, DiscoveryLocalizedTextAssetParser>(["Discoveries"]);

        services.AddOrderingTextAssetPatcher<EnemyLeaf, EnemyOrderingTextAssetParser>("TattleList");
        services.AddLocalizedTextAssetPatcher<EnemyLeaf, EnemyLocalizedTextAssetParser>(["EnemyTattle"]);
        services.AddTextAssetPatcher<EnemyLeaf, EnemyTextAssetParser>(["EnemyData"]);
        
        services.AddOrderingTextAssetPatcher<RecordLeaf, RecordOrderingTextAssetParser>("SynopsisOrder");
        services.AddLocalizedTextAssetPatcher<RecordLeaf, RecordLocalizedTextAssetParser>(["Synopsis"]);
        
        services.AddTextAssetPatcher<TermacadePrizeLeaf, TermacadePrizeTextAssetParser>(["Termacade"]);
        
        services.AddTextAssetPatcher<RecipeLeaf, RecipeTextAssetParser>(["RecipeData"]);
        services.AddTextAssetPatcher<RecipeLibraryEntryLeaf, RecipeLibraryEntryTextAssetParser>(
            ["CookOrder", "CookLibrary"]);

        services.AddLocalizedTextAssetPatcher<AreaLeaf, AreaLocalizedTextAssetParser>(
            ["AreaNames", "AreaDesc"]);

        services.AddSingleton<IResourcesTypePatcher<TextAsset>, RootTextAssetPatcher>();
        services.AddSingleton<IResourcesArrayTypePatcher<Sprite>, RootSpritesArrayPatcher>();

        services.AddSingleton<ITopLevelPatcher, ResourcesTopLevelPatcher>();
        services.AddSingleton<ITopLevelPatcher, GlobalFlagsCapsTopLevelPatcher>();
        services.AddSingleton<ITopLevelPatcher, CrystalBerriesAmountTopLevelPatcher>();
        services.AddSingleton<ITopLevelPatcher, ItemAndMedalSpriteTopLevelPatcher>();
        services.AddSingleton<ITopLevelPatcher, PrizeMedalsTopLevelPatcher>();
        services.AddSingleton<ITopLevelPatcher, LibraryCapsTopLevelPatcher>();
        services.AddSingleton<ITopLevelPatcher, EnemyEncounterCapTopLevelPatcher>();
        services.AddSingleton<ITopLevelPatcher, EventControlExcludeIdsTopLevelPatcher>();
        services.AddSingleton<ITopLevelPatcher, CaveOfTrialsRandomModeExclusionTopLevelPatcher>();
        services.AddSingleton<ITopLevelPatcher, RareSpyDataTopLevelPatcher>();
        services.AddSingleton<ITopLevelPatcher, SpyDialoguePauseMenuTopLevelPatcher>();
        services.AddSingleton<ITopLevelPatcher, AreaMapPositionsTopLevelPatcher>();
        services.AddSingleton<RootPatcher>();

        services.AddSingleton<IAssemblyCSharpDataCollector, AssemblyCSharpDataCollector>();
        services.AddSingleton<IBaseGameCollector, BaseGameItemsCollector>();
        services.AddSingleton<IBaseGameCollector, BaseGameEnemiesCollector>();
        services.AddSingleton<IBaseGameCollector, BaseGameRecipesCollector>();
        services.AddSingleton<IBaseGameCollector, BaseGameRecipeLibraryEntriesCollector>();
        services.AddSingleton<IBaseGameCollector, BaseGameAreasCollector>();
        services.AddSingleton<IBaseGameCollector, BaseGameMedalsCollector>();
        services.AddSingleton<IBaseGameCollector, BaseGamePrizeMedalsCollector>();
        services.AddSingleton<IBaseGameCollector, BaseGameGlobalFlagsCollector>();
        services.AddSingleton<IBaseGameCollector, BaseGameCrystalBerriesCollector>();
        services.AddSingleton<IBaseGameCollector, BaseGameCommonDialoguesCollector>();
        services.AddSingleton<IBaseGameCollector, BaseGameMenuTextsCollector>();
        services.AddSingleton<IBaseGameCollector, BaseGameDiscoveriesCollector>();
        services.AddSingleton<IBaseGameCollector, BaseGameRecordsCollector>();
        services.AddSingleton<IBaseGameCollector, BaseGameTermacadePrizesCollector>();
        services.AddSingleton<RootBaseGameDataCollector>();

        services.AddSingleton<IGlobalMonoBehaviourExecution, GlobalMonoBehaviourExecution>();

        services.AddSingleton<IBudConfigManager, BudConfigManager>();
        services.AddSingleton<IVenusFactory, VenusFactory>();
        services.AddSingleton<IBudsDiscoverer, BudsDiscoverer>();
        services.AddSingleton<IBudsValidator, BudsValidator>();
        services.AddSingleton<IBudsDependencySorter, BudsDependencySorter>();
        services.AddSingleton<IBudsLoadOrderEnumerator, BudsLoadOrderEnumerator>();
        services.AddSingleton<IAssemblyLoader, AssemblyLoader>();
        services.AddSingleton<BudLoader>();

        return services.BuildServiceProvider();
    }
}