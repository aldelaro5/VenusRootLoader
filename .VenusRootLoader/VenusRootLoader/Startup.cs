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
using VenusRootLoader.Patching.Logic.BaseGameFixes;
using VenusRootLoader.Patching.Logic.LeavesSupport;
using VenusRootLoader.Patching.Logic.LimitsRemoval;
using VenusRootLoader.Patching.PostBudLoading;
using VenusRootLoader.Patching.Resources;
using VenusRootLoader.Patching.Resources.AudioClipPatchers;
using VenusRootLoader.Patching.Resources.PrefabPatchers;
using VenusRootLoader.Patching.Resources.SpritesPatchers;
using VenusRootLoader.Patching.Resources.TextAssetPatchers;
using VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;
using VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers.GlobalData;
using VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers.LocalisedData;
using VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers.OrderingData;
using VenusRootLoader.Persistence;
using VenusRootLoader.Persistence.BaseGameSave;
using VenusRootLoader.Persistence.BudsSave;
using VenusRootLoader.Registry;
using VenusRootLoader.Unity;
using VenusRootLoader.Utility;
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
                SaveDataPath = fileSystem.Path.Combine(basePath, "VrlSaveData"),
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

        services.AddAutoSequentialIdBasedLeavesRegistry<LanguageLeaf>();
        services.AddAutoSequentialIdBasedLeavesRegistry<EventLeaf>();
        services.AddAutoSequentialIdBasedLeavesRegistry<DialogueBleepLeaf>();
        services.AddEnumBasedLeavesRegistry<AnimIdLeaf, MainManager.AnimIDs>(-1);
        services.AddEnumBasedLeavesRegistry<ItemLeaf, MainManager.Items>();
        services.AddEnumBasedLeavesRegistryWithOrdering<MedalLeaf, MainManager.BadgeTypes>();
        services.AddAutoSequentialIdBasedLeavesRegistry<BattleEventDialogueLeaf>();
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
        services.AddEnumBasedLeavesRegistry<MusicLeaf, MainManager.Musics>();
        services.AddEnumBasedLeavesRegistry<QuestLeaf, MainManager.BoardQuests>();
        services.AddAutoSequentialIdBasedLeavesRegistry<RankBonusLeaf>();
        services.AddAutoSequentialIdBasedLeavesRegistry<LoreBookLeaf>();
        services.AddAutoSequentialIdBasedLeavesRegistry<ActionCommandHelpTextLeaf>();
        services.AddEnumBasedLeavesRegistry<SkillLeaf, MainManager.Skills>();
        services.AddAutoSequentialIdBasedLeavesRegistry<FishingTextLeaf>();
        services.AddAutoSequentialIdBasedLeavesRegistry<SpyCardsTextLeaf>();
        services.AddAutoSequentialIdBasedLeavesRegistryWithOrdering<SpyCardLeaf>();
        services.AddAutoSequentialIdBasedLeavesRegistry<MedalShopLeaf>();
        services.AddAutoSequentialIdBasedLeavesRegistry<CuttableGrassLeaf>();
        services.AddEnumBasedLeavesRegistry<MapLeaf, MainManager.Maps>();

        services.AddSingleton<ISpriteArrayPatcher, EnemyPortraitsSpriteArrayPatcher>(provider =>
            new(
                [ResourcesPaths.SpritesEnemyPortraitsPath],
                provider.GetRequiredService<ILeavesRegistry<DiscoveryLeaf>>(),
                provider.GetRequiredService<ILeavesRegistry<EnemyLeaf>>(),
                provider.GetRequiredService<ILeavesRegistry<RecordLeaf>>(),
                provider.GetRequiredService<ILeavesRegistry<QuestLeaf>>()));

        services.AddSingleton<ISpriteArrayPatcher, GrassSpritesArrayPatcher>(provider =>
            new(
                [$"{ResourcesPaths.SpritesObjectsGrassPath}"],
                provider.GetRequiredService<ILeavesRegistry<CuttableGrassLeaf>>()));

        services.AddSingleton<IAudioClipPatcher, SoundDialoguesAudioClipPatcher>(provider =>
            new(
                [ResourcesPaths.AudioSoundsDialogueDirectory],
                provider.GetRequiredService<ILeavesRegistry<DialogueBleepLeaf>>()));
        services.AddSingleton<IAudioClipArrayPatcher, SoundDialoguesAudioClipArrayPatcher>(provider =>
            new(
                [ResourcesPaths.AudioSoundsDialogueDirectory],
                provider.GetRequiredService<ILeavesRegistry<DialogueBleepLeaf>>()));

        services.AddSingleton<IAudioClipPatcher, MusicAudioClipPatcher>(provider =>
            new([ResourcesPaths.AudioMusicDirectory], provider.GetRequiredService<ILeavesRegistry<MusicLeaf>>()));

        services.AddSingleton<ITextAssetDumper, TextAssetDumper>();

        services.AddTextAssetPatcher<AnimIdLeaf, AnimIdTextAssetParser>([ResourcesPaths.DataAnimIdsPath]);

        services.AddTextAssetPatcher<ItemLeaf, ItemTextAssetParser>([ResourcesPaths.DataItemsPath]);
        services.AddLocalizedTextAssetPatcher<ItemLeaf, ItemLocalizedTextAssetParser>(
            [ResourcesPaths.DataLocalizedItemsPathSuffix]);

        services.AddTextAssetPatcher<MedalLeaf, MedalTextAssetParser>([ResourcesPaths.DataMedalsPath]);
        services.AddOrderingTextAssetPatcher<MedalLeaf, MedalOrderingTextAssetParser>(
            ResourcesPaths.DataMedalsOrderingPath);
        services.AddLocalizedTextAssetPatcher<MedalLeaf, MedalLocalizedTextAssetParser>(
            [ResourcesPaths.DataLocalizedMedalPathSuffix]);

        services.AddLocalizedTextAssetPatcher<CrystalBerryLeaf, CrystalBerryLocalizedTextAssetParser>(
            [ResourcesPaths.DataLocalizedCrystalBerryFortuneTellerHintsPathSuffix]);
        services
            .AddLocalizedTextAssetPatcher<MedalFortuneTellerHintLeaf, MedalFortuneTellerHintLocalizedTextAssetParser>(
                [ResourcesPaths.DataLocalizedMedalFortuneTellerHintsPathSuffix]);

        services.AddLocalizedTextAssetPatcher<CommonDialogueLeaf, CommonDialogueLocalizedTextAssetParser>(
            [ResourcesPaths.DataLocalizedCommonDialoguesPathSuffix],
            r => r.OrderBy(l => l.InternalGameIndex));

        services.AddLocalizedTextAssetPatcher<MenuTextLeaf, MenuTextLocalizedTextAssetParser>(
            [ResourcesPaths.DataLocalizedMenuTextsPathSuffix]);

        services.AddOrderingTextAssetPatcher<DiscoveryLeaf, DiscoveryOrderingTextAssetParser>(
            ResourcesPaths.DataDiscoveriesOrderingPath);
        services.AddLocalizedTextAssetPatcher<DiscoveryLeaf, DiscoveryLocalizedTextAssetParser>(
            [ResourcesPaths.DataLocalizedDiscoveriesPathSuffix]);

        services.AddOrderingTextAssetPatcher<EnemyLeaf, EnemyOrderingTextAssetParser>(
            ResourcesPaths.DataBestiaryEntriesOrderingPath);
        services.AddLocalizedTextAssetPatcher<EnemyLeaf, EnemyLocalizedTextAssetParser>(
            [ResourcesPaths.DataLocalizedBestiaryEntriesPathSuffix]);
        services.AddTextAssetPatcher<EnemyLeaf, EnemyTextAssetParser>([ResourcesPaths.DataEnemiesPath]);

        services.AddOrderingTextAssetPatcher<RecordLeaf, RecordOrderingTextAssetParser>(
            ResourcesPaths.DataRecordsOrderingPath);
        services.AddLocalizedTextAssetPatcher<RecordLeaf, RecordLocalizedTextAssetParser>(
            [ResourcesPaths.DataLocalizedRecordsPathSuffix]);

        services.AddTextAssetPatcher<TermacadePrizeLeaf, TermacadePrizeTextAssetParser>(
            [ResourcesPaths.DataTermacadePrizesPath]);

        services.AddTextAssetPatcher<RecipeLeaf, RecipeTextAssetParser>([ResourcesPaths.DataRecipesPath]);
        services.AddTextAssetPatcher<RecipeLibraryEntryLeaf, RecipeLibraryEntryTextAssetParser>(
        [
            ResourcesPaths.DataRecipesLibraryEntriesResultItemsPath,
            ResourcesPaths.DataRecipesLibraryEntriesInputItemsPath
        ]);

        services.AddLocalizedTextAssetPatcher<AreaLeaf, AreaLocalizedTextAssetParser>(
        [
            ResourcesPaths.DataLocalizedAreaNamesPathSuffix,
            ResourcesPaths.DataLocalizedAreaDescriptionsPathSuffix
        ]);

        services.AddTextAssetPatcher<MusicLeaf, MusicTextAssetParser>([ResourcesPaths.DataMusicLoopPointsPath]);
        services.AddLocalizedTextAssetPatcher<MusicLeaf, MusicLocalizedTextAssetParser>(
            [ResourcesPaths.DataLocalizedMusicNamesPathSuffix]);

        services.AddTextAssetPatcher<QuestLeaf, QuestTextAssetParser>(
            [ResourcesPaths.DataQuestsPath, ResourcesPaths.DataQuestsRequirementsPath]);
        services.AddLocalizedTextAssetPatcher<QuestLeaf, QuestLocalizedTextAssetParser>(
            [ResourcesPaths.DataLocalizedQuestsPathSuffix]);

        services.AddTextAssetPatcher<RankBonusLeaf, RankBonusTextAssetParser>(
            [ResourcesPaths.DataRankBonusesPath]);

        services.AddLocalizedTextAssetPatcher<LoreBookLeaf, LoreBookLocalizedTextAssetParser>(
        [
            ResourcesPaths.DataLocalizedLoreBooksPathSuffix,
            ResourcesPaths.DataLocalizedLoreBookFortuneTellerHintsPathSuffix
        ]);

        services.AddLocalizedTextAssetPatcher<ActionCommandHelpTextLeaf, ActionCommandHelpTextLocalizedTextAssetParser>(
            [ResourcesPaths.DataLocalizedActionCommandHelpTextsPathSuffix]);

        services.AddTextAssetPatcher<SkillLeaf, SkillTextAssetParser>([ResourcesPaths.DataSkillsPath]);
        services.AddLocalizedTextAssetPatcher<SkillLeaf, SkillLocalizedTextAssetParser>(
            [ResourcesPaths.DataLocalizedSkillsPathSuffix]);

        services.AddLocalizedTextAssetPatcher<FishingTextLeaf, FishingTextLocalizedTextAssetParser>(
            [ResourcesPaths.DataLocalizedFishingTextsPathSuffix]);

        services.AddLocalizedTextAssetPatcher<SpyCardsTextLeaf, SpyCardsTextLocalizedTextAssetParser>(
            [ResourcesPaths.DataLocalizedSpyCardsTextsPathSuffix]);

        services.AddOrderingTextAssetPatcher<SpyCardLeaf, SpyCardOrderingTextAssetParser>(
            ResourcesPaths.DataSpyCardsOrderingPath);
        services.AddTextAssetPatcher<SpyCardLeaf, SpyCardTextAssetParser>([ResourcesPaths.DataSpyCardsPath]);
        services.AddLocalizedTextAssetPatcher<SpyCardLeaf, SpyCardLocalizedTextAssetParser>(
            [ResourcesPaths.DataLocalizedSpyCardsPathSuffix]);

        services.AddSingleton<IMapEntityTextAssetParser, MapEntityTextAssetParser>();
        services.AddSingleton<IMapEntityTextAssetPatcher, MapEntitiesTextAssetPatcher>();
        services.AddSingleton<IMapDialoguesTextAssetPatcher, MapDialoguesTextAssetPatcher>();

        services.AddSingleton<IResourcesTypePatcher<TextAsset>, RootTextAssetPatcher>();
        services.AddSingleton<IResourcesTypePatcher<AudioClip>, RootAudioClipPatcher>();
        services.AddSingleton<IResourcesTypePatcher<Object>, RootPrefabPatcher>();
        services.AddSingleton<IResourcesArrayTypePatcher<Sprite>, RootSpritesArrayPatcher>();
        services.AddSingleton<IResourcesArrayTypePatcher<AudioClip>, RootAudioClipsArrayPatcher>();

        services.AddScoped<IAssemblyCSharpDataCollector, AssemblyCSharpDataCollector>();
        services.AddScoped<IBaseGameCollector, LanguagesCollector>();
        services.AddScoped<IBaseGameCollector, EventCollector>();
        services.AddScoped<IBaseGameCollector, DialogueBleepCollector>();
        services.AddScoped<IBaseGameCollector, AnimIdsCollector>();
        services.AddScoped<IBaseGameCollector, ItemsCollector>();
        services.AddScoped<IBaseGameCollector, BattleEventDialoguesCollector>();
        services.AddScoped<IBaseGameCollector, EnemiesCollector>();
        services.AddScoped<IBaseGameCollector, RecipesCollector>();
        services.AddScoped<IBaseGameCollector, RecipeLibraryEntriesCollector>();
        services.AddScoped<IBaseGameCollector, AreasCollector>();
        services.AddScoped<IBaseGameCollector, MedalsCollector>();
        services.AddScoped<IBaseGameCollector, GlobalFlagsCollector>();
        services.AddScoped<IBaseGameCollector, PrizeMedalsCollector>();
        services.AddScoped<IBaseGameCollector, CrystalBerriesCollector>();
        services.AddScoped<IBaseGameCollector, CommonDialoguesCollector>();
        services.AddScoped<IBaseGameCollector, MedalFortuneTellerHintCollector>();
        services.AddScoped<IBaseGameCollector, MenuTextsCollector>();
        services.AddScoped<IBaseGameCollector, DiscoveriesCollector>();
        services.AddScoped<IBaseGameCollector, RecordsCollector>();
        services.AddScoped<IBaseGameCollector, TermacadePrizesCollector>();
        services.AddScoped<IBaseGameCollector, MusicsCollector>();
        services.AddScoped<IBaseGameCollector, QuestsCollector>();
        services.AddScoped<IBaseGameCollector, RankBonusesCollector>();
        services.AddScoped<IBaseGameCollector, LoreBooksCollector>();
        services.AddScoped<IBaseGameCollector, ActionCommandHelpTextsCollector>();
        services.AddScoped<IBaseGameCollector, SkillsCollector>();
        services.AddScoped<IBaseGameCollector, FishingTextsCollector>();
        services.AddScoped<IBaseGameCollector, SpyCardsTextsCollector>();
        services.AddScoped<IBaseGameCollector, SpyCardsCollector>();
        services.AddScoped<IBaseGameCollector, MedalShopsCollector>();
        services.AddScoped<IBaseGameCollector, CuttableGrassCollector>();
        services.AddScoped<IBaseGameCollector, MapsCollector>();
        services.AddScoped<RootCollector>();

        services.AddSingleton<IGameDataRuntimeState, GameDataRuntimeState>();
        services.AddSingleton<IBaseGameSaveDataSerializer, BaseGameSaveDataSerializer>();
        services.AddSingleton<IBaseGameSaveDataDeserializer, BaseGameSaveDataDeserializer>();
        services.AddSingleton<IBudsSaveDataSerializer, BudsSaveDataSerializer>();
        services.AddSingleton<IBudsSaveDataDeserializer, BudsSaveDataDeserializer>();
        services.AddSingleton<ISaveDataPersistence, SaveDataPersistence>();

        services.AddSingleton<IGlobalMonoBehaviourExecution, GlobalMonoBehaviourExecution>();
        services.AddSingleton<IBudConfigManager, BudConfigManager>();
        services.AddSingleton<IVenusFactory, VenusFactory>();
        services.AddSingleton<IBudsDiscoverer, BudsDiscoverer>();
        services.AddSingleton<IBudsValidator, BudsValidator>();
        services.AddSingleton<IBudsDependencySorter, BudsDependencySorter>();
        services.AddSingleton<IBudsLoadOrderEnumerator, BudsLoadOrderEnumerator>();
        services.AddSingleton<IAssemblyLoader, AssemblyLoader>();
        services.AddSingleton<IBudLoader, BudLoader>();

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
        services.AddSingleton<ITopLevelPatcher, MedalShopsTopLevelPatcher>();
        services.AddSingleton<ITopLevelPatcher, PathNodesActionBehaviorsTopLevelPatcher>();
        services.AddSingleton<ITopLevelPatcher, MapEntitiesArraysLengthTopLevelPatcher>();
        services.AddSingleton<ITopLevelPatcher, EntityIsKillLastPositionTopLevelPatcher>();
        services.AddSingleton<ITopLevelPatcher, MemoryAllocationsTopLevelPatcher>();
        services.AddSingleton<ITopLevelPatcher, MapsLoadingTopLevelPatcher>();
        services.AddSingleton<ITopLevelPatcher, SaveDataPersistenceTopLevelPatcher>();
        services.AddSingleton<ITopLevelPatcher, CollectibleMedalNearEnemyEncounterTopLevelPatcher>();
        services.AddSingleton<ITopLevelPatcher, RecordsUnlockTopLevelPatcher>();
        services.AddSingleton<ITopLevelPatcher, HazardsMapYLimitTopLevelPatcher>();

        services.AddSingleton<ITopLevelPatcher, BudLoaderTopLevelPatcher>();

        services.AddSingleton<ITopLevelPatcher, LoreBooksAmountTopLevelPatcher>();
        services.AddSingleton<RootPatcher>();

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        RegistryResolver.Init(serviceProvider);

        return serviceProvider;
    }
}