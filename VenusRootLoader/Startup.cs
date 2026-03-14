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
using VenusRootLoader.Patching.Resources.AudioClipPatchers;
using VenusRootLoader.Patching.Resources.PrefabPatchers;
using VenusRootLoader.Patching.Resources.SpritesPatchers;
using VenusRootLoader.Patching.Resources.TextAssetPatchers;
using VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers.GlobalData;
using VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers.LocalisedData;
using VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers.OrderingData;
using VenusRootLoader.Registry;
using VenusRootLoader.Unity;
using VenusRootLoader.Unity.CustomAudioClip;
using Object = UnityEngine.Object;

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

        services.AddEnumBasedLeavesRegistry<ItemLeaf, MainManager.Items>();
        services.AddEnumBasedLeavesRegistryWithOrdering<MedalLeaf, MainManager.BadgeTypes>();
        services.AddEnumBasedLeavesRegistryWithOrdering<EnemyLeaf, MainManager.Enemies>();
        services.AddAutoSequentialIdBasedLeavesRegistry<RecipeLeaf>();
        services.AddAutoSequentialIdBasedLeavesRegistry<RecipeLibraryEntryLeaf>();
        services.AddEnumBasedLeavesRegistry<AreaLeaf, MainManager.Areas>();
        services.AddAutoSequentialIdBasedLeavesRegistry<FlagLeaf>();
        services.AddAutoSequentialIdBasedLeavesRegistry<FlagvarLeaf>();
        services.AddAutoSequentialIdBasedLeavesRegistry<FlagstringLeaf>();
        services.AddAutoSequentialIdBasedLeavesRegistry<CrystalBerryLeaf>();
        services.AddAutoSequentialIdBasedLeavesRegistry<MedalFortuneTellerHintLeaf>();
        services.AddAutoSequentialIdBasedLeavesRegistry<CommonDialogueLeaf>(IdSequenceDirection.Decrement, -1);
        services.AddAutoSequentialIdBasedLeavesRegistry<MenuTextLeaf>();
        services.AddAutoSequentialIdBasedLeavesRegistry<PrizeMedalLeaf>();
        services.AddAutoSequentialIdBasedLeavesRegistryWithOrdering<DiscoveryLeaf>();
        services.AddAutoSequentialIdBasedLeavesRegistryWithOrdering<RecordLeaf>();
        services.AddAutoSequentialIdBasedLeavesRegistry<TermacadePrizeLeaf>();
        services.AddAutoSequentialIdBasedLeavesRegistry<DialogueBleepLeaf>();
        services.AddEnumBasedLeavesRegistry<MusicLeaf, MainManager.Musics>();
        services.AddEnumBasedLeavesRegistry<QuestLeaf, MainManager.BoardQuests>();
        services.AddAutoSequentialIdBasedLeavesRegistry<RankBonusLeaf>();
        services.AddAutoSequentialIdBasedLeavesRegistry<LoreBookLeaf>();
        services.AddAutoSequentialIdBasedLeavesRegistry<ActionCommandHelpTextLeaf>();
        services.AddEnumBasedLeavesRegistry<SkillLeaf, MainManager.Skills>();
        services.AddAutoSequentialIdBasedLeavesRegistry<FishingTextLeaf>();
        services.AddAutoSequentialIdBasedLeavesRegistry<TestRoomTextLeaf>();
        services.AddAutoSequentialIdBasedLeavesRegistry<SpyCardsTextLeaf>();
        services.AddAutoSequentialIdBasedLeavesRegistryWithOrdering<SpyCardLeaf>();
        services.AddSingleton<IRegistryResolver, RegistryResolver>();

        services.AddSingleton<IPrefabPatcher, MapPatcher>(provider =>
            new(["Maps"], provider.GetRequiredService<ILeavesRegistry<MusicLeaf>>()));

        services.AddSingleton<ISpriteArrayPatcher, EnemyPortraitsSpriteArrayPatcher>(provider =>
            new(
                ["Items/EnemyPortraits"],
                provider.GetRequiredService<ILeavesRegistry<DiscoveryLeaf>>(),
                provider.GetRequiredService<ILeavesRegistry<EnemyLeaf>>(),
                provider.GetRequiredService<ILeavesRegistry<RecordLeaf>>(),
                provider.GetRequiredService<ILeavesRegistry<QuestLeaf>>()));

        services.AddSingleton<IAudioClipPatcher, SoundDialoguesAudioClipPatcher>(provider =>
            new(["Sounds/Dialogue"], provider.GetRequiredService<ILeavesRegistry<DialogueBleepLeaf>>()));
        services.AddSingleton<IAudioClipArrayPatcher, SoundDialoguesAudioClipArrayPatcher>(provider =>
            new(["Sounds/Dialogue"], provider.GetRequiredService<ILeavesRegistry<DialogueBleepLeaf>>()));

        services.AddSingleton<IAudioClipPatcher, MusicAudioClipPatcher>(provider =>
            new(["Music"], provider.GetRequiredService<ILeavesRegistry<MusicLeaf>>()));

        services.AddTextAssetPatcher<ItemLeaf, ItemTextAssetParser>(["ItemData"]);
        services.AddLocalizedTextAssetPatcher<ItemLeaf, ItemLocalizedTextAssetParser>(["Items"]);

        services.AddTextAssetPatcher<MedalLeaf, MedalTextAssetParser>(["BadgeData"]);
        services.AddOrderingTextAssetPatcher<MedalLeaf, MedalOrderingTextAssetParser>("BadgeOrder");
        services.AddLocalizedTextAssetPatcher<MedalLeaf, MedalLocalizedTextAssetParser>(["BadgeName"]);

        services.AddLocalizedTextAssetPatcher<CrystalBerryLeaf, CrystalBerryLocalizedTextAssetParser>(
            ["FortuneTeller0"]);
        services
            .AddLocalizedTextAssetPatcher<MedalFortuneTellerHintLeaf, MedalFortuneTellerHintLocalizedTextAssetParser>(
                ["FortuneTeller2"]);

        services.AddLocalizedTextAssetPatcher<CommonDialogueLeaf, CommonDialogueLocalizedTextAssetParser>(
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

        services.AddTextAssetPatcher<MusicLeaf, MusicTextAssetParser>(["LoopPoints"]);
        services.AddLocalizedTextAssetPatcher<MusicLeaf, MusicLocalizedTextAssetParser>(["MusicList"]);

        services.AddTextAssetPatcher<QuestLeaf, QuestTextAssetParser>(["BoardData", "QuestChecks"]);
        services.AddLocalizedTextAssetPatcher<QuestLeaf, QuestLocalizedTextAssetParser>(["BoardQuests"]);

        services.AddTextAssetPatcher<RankBonusLeaf, RankBonusTextAssetParser>(["LevelData"]);

        services.AddLocalizedTextAssetPatcher<LoreBookLeaf, LoreBookLocalizedTextAssetParser>(
            ["LoreText", "FortuneTeller1"]);

        services.AddLocalizedTextAssetPatcher<ActionCommandHelpTextLeaf, ActionCommandHelpTextLocalizedTextAssetParser>(
            ["ActionCommands"]);

        services.AddTextAssetPatcher<SkillLeaf, SkillTextAssetParser>(["SkillData"]);
        services.AddLocalizedTextAssetPatcher<SkillLeaf, SkillLocalizedTextAssetParser>(["Skills"]);

        services.AddLocalizedTextAssetPatcher<FishingTextLeaf, FishingTextLocalizedTextAssetParser>(
            ["Fishing"]);

        services.AddTextAssetPatcher<TestRoomTextLeaf, TestRoomTextTextAssetParser>(["TestRoom"]);

        services.AddLocalizedTextAssetPatcher<SpyCardsTextLeaf, SpyCardsTextLocalizedTextAssetParser>(
            ["CardDialogue"]);

        services.AddOrderingTextAssetPatcher<SpyCardLeaf, SpyCardOrderingTextAssetParser>("CardOrder");
        services.AddTextAssetPatcher<SpyCardLeaf, SpyCardTextAssetParser>(["CardData"]);
        services.AddLocalizedTextAssetPatcher<SpyCardLeaf, SpyCardLocalizedTextAssetParser>(["CardText"]);

        services.AddSingleton<IResourcesTypePatcher<TextAsset>, RootTextAssetPatcher>();
        services.AddSingleton<IResourcesTypePatcher<AudioClip>, RootAudioClipPatcher>();
        services.AddSingleton<IResourcesTypePatcher<Object>, RootPrefabPatcher>();
        services.AddSingleton<IResourcesArrayTypePatcher<Sprite>, RootSpritesArrayPatcher>();
        services.AddSingleton<IResourcesArrayTypePatcher<AudioClip>, RootAudioClipsArrayPatcher>();

        services.AddSingleton<ITopLevelPatcher, ResourcesTopLevelPatcher>();
        services.AddSingleton<ITopLevelPatcher, GlobalFlagsCapsTopLevelPatcher>();
        services.AddSingleton<ITopLevelPatcher, CrystalBerriesAmountTopLevelPatcher>();
        services.AddSingleton<ITopLevelPatcher, ItemAndMedalSpriteTopLevelPatcher>();
        services.AddSingleton<ITopLevelPatcher, PrizeMedalsTopLevelPatcher>();
        services.AddSingleton<ITopLevelPatcher, LibraryCapsTopLevelPatcher>();
        services.AddSingleton<ITopLevelPatcher, EnemyEncounterCapTopLevelPatcher>();
        services.AddSingleton<ITopLevelPatcher, EventControlExcludeIdsTopLevelPatcher>();
        services.AddSingleton<ITopLevelPatcher, CaveOfTrialsRandomModeExclusionTopLevelPatcher>();
        services.AddSingleton<ITopLevelPatcher, FortuneTellerHintFlagsTopLevelPatcher>();
        services.AddSingleton<ITopLevelPatcher, RareSpyDataTopLevelPatcher>();
        services.AddSingleton<ITopLevelPatcher, SpyDialoguePauseMenuTopLevelPatcher>();
        services.AddSingleton<ITopLevelPatcher, AreaMapPositionsTopLevelPatcher>();
        services.AddSingleton<ITopLevelPatcher, NonPurchasableMusicsTopLevelPatcher>();
        services.AddSingleton<ITopLevelPatcher, UndergroundBarQuestsTopLevelPatcher>();
        services.AddSingleton<RootPatcher>();

        services.AddSingleton<IAssemblyCSharpDataCollector, AssemblyCSharpDataCollector>();
        services.AddSingleton<IBaseGameCollector, ItemsCollector>();
        services.AddSingleton<IBaseGameCollector, EnemiesCollector>();
        services.AddSingleton<IBaseGameCollector, RecipesCollector>();
        services.AddSingleton<IBaseGameCollector, RecipeLibraryEntriesCollector>();
        services.AddSingleton<IBaseGameCollector, AreasCollector>();
        services.AddSingleton<IBaseGameCollector, MedalsCollector>();
        services.AddSingleton<IBaseGameCollector, PrizeMedalsCollector>();
        services.AddSingleton<IBaseGameCollector, GlobalFlagsCollector>();
        services.AddSingleton<IBaseGameCollector, CrystalBerriesCollector>();
        services.AddSingleton<IBaseGameCollector, CommonDialoguesCollector>();
        services.AddSingleton<IBaseGameCollector, MedalFortuneTellerHintCollector>();
        services.AddSingleton<IBaseGameCollector, MenuTextsCollector>();
        services.AddSingleton<IBaseGameCollector, DiscoveriesCollector>();
        services.AddSingleton<IBaseGameCollector, RecordsCollector>();
        services.AddSingleton<IBaseGameCollector, TermacadePrizesCollector>();
        services.AddSingleton<IBaseGameCollector, DialogueBleepCollector>();
        services.AddSingleton<IBaseGameCollector, MusicsCollector>();
        services.AddSingleton<IBaseGameCollector, QuestsCollector>();
        services.AddSingleton<IBaseGameCollector, RankBonusesCollector>();
        services.AddSingleton<IBaseGameCollector, LoreBooksCollector>();
        services.AddSingleton<IBaseGameCollector, ActionCommandHelpTextsCollector>();
        services.AddSingleton<IBaseGameCollector, SkillsCollector>();
        services.AddSingleton<IBaseGameCollector, FishingTextsCollector>();
        services.AddSingleton<IBaseGameCollector, TestRoomTextsCollector>();
        services.AddSingleton<IBaseGameCollector, SpyCardsTextsCollector>();
        services.AddSingleton<IBaseGameCollector, SpyCardsCollector>();
        services.AddSingleton<RootCollector>();

        services.AddSingleton<IGlobalMonoBehaviourExecution, GlobalMonoBehaviourExecution>();
        services.AddSingleton<ICustomAudioClipProvider, CustomAudioClipProvider>();

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