using HarmonyLib;
using Microsoft.Extensions.Logging;
using Mono.Cecil;
using MonoMod.Cil;
using MonoMod.Utils;
using System.Reflection;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;
using VenusRootLoader.Registry;
using VenusRootLoader.Unity.AssetLoading;
using VenusRootLoader.Utility;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class EnemiesCollector : IBaseGameCollector
{
    private readonly string[] _enemiesData = RootCollector.ReadTextAssetLines(ResourcesPaths.DataEnemiesPath);

    private readonly string _enemiesOrderingData =
        RootCollector.ReadWholeTextAsset(ResourcesPaths.DataBestiaryEntriesOrderingPath);

    private readonly Dictionary<int, string[]> _enemiesLanguageData =
        RootCollector.ReadLocalizedTestAssetLines(ResourcesPaths.DataLocalizedBestiaryEntriesPathSuffix);

    private readonly string[] _enemyNamedIds = Enum.GetNames(typeof(MainManager.Enemies)).ToArray();

    private readonly ILogger<EnemiesCollector> _logger;
    private readonly IAssemblyCSharpDataCollector _assemblyCSharpDataCollector;
    private readonly ITextAssetParser<HasEnemyLeaf> _enemyTextAssetParser;
    private readonly IOrderedLeavesRegistry<HasEnemyLeaf> _orderedRegistry;
    private readonly ILeavesRegistry<LanguageLeaf> _languageRegistry;
    private readonly IOrderingTextAssetParser<HasEnemyLeaf> _enemyOrderingTextAssetParser;
    private readonly ILocalizedTextAssetParser<HasEnemyLeaf> _enemyLocalizedTextAssetParser;

    public EnemiesCollector(
        ILogger<EnemiesCollector> logger,
        IAssemblyCSharpDataCollector assemblyCSharpDataCollector,
        ITextAssetParser<HasEnemyLeaf> enemyTextAssetParser,
        IOrderedLeavesRegistry<HasEnemyLeaf> orderedRegistry,
        ILeavesRegistry<LanguageLeaf> languageRegistry,
        IOrderingTextAssetParser<HasEnemyLeaf> enemyOrderingTextAssetParser,
        ILocalizedTextAssetParser<HasEnemyLeaf> enemyLocalizedTextAssetParser)
    {
        _logger = logger;
        _assemblyCSharpDataCollector = assemblyCSharpDataCollector;
        _orderedRegistry = orderedRegistry;
        _enemyTextAssetParser = enemyTextAssetParser;
        _enemyOrderingTextAssetParser = enemyOrderingTextAssetParser;
        _enemyLocalizedTextAssetParser = enemyLocalizedTextAssetParser;
        _languageRegistry = languageRegistry;
    }

    public void CollectBaseGameData()
    {
        _enemyOrderingTextAssetParser.FromTextAssetString(_enemiesOrderingData, _orderedRegistry);

        for (int i = 0; i < _enemyNamedIds.Length; i++)
        {
            string enemyNamedId = _enemyNamedIds[i];

            if (_orderedRegistry.BaseGameIdsToOrderingIndex.ContainsKey(i))
                _orderedRegistry.RegisterExistingWithOrdering(i, enemyNamedId);
            else
                _orderedRegistry.Registry.RegisterExisting(i, enemyNamedId);
        }

        IMetadataTokenProvider tokenBossList = null!;
        IMetadataTokenProvider tokenMiniBossLiss = null!;
        IMetadataTokenProvider tokenMiniBossCard = null!;
        IMetadataTokenProvider tokenSpecialList = null!;

        MethodBase eventControlCctor =
            AccessTools.Constructor(typeof(EventControl), null, true)!;
        using DynamicMethodDefinition dmd = new(eventControlCctor);
        ILContext context = new(dmd.Definition);
        ILCursor cursor = new(context);

        cursor
            .GotoNext(i => i.MatchStsfld<EventControl>(nameof(EventControl.bosslist)))
            .GotoPrev(i => i.MatchLdtoken(out tokenBossList!))
            .Goto(0)
            .GotoNext(i => i.MatchStsfld<EventControl>(nameof(EventControl.minibosslist)))
            .GotoPrev(i => i.MatchLdtoken(out tokenMiniBossLiss!))
            .Goto(0)
            .GotoNext(i => i.MatchStsfld<EventControl>(nameof(EventControl.minibosscard)))
            .GotoPrev(i => i.MatchLdtoken(out tokenMiniBossCard!))
            .Goto(0)
            .GotoNext(i => i.MatchStsfld<EventControl>(nameof(EventControl.speciallist)))
            .GotoPrev(i => i.MatchLdtoken(out tokenSpecialList!));

        FieldInfo bossListField = ((FieldReference)tokenBossList).ResolveReflection();
        FieldInfo miniBossListField = ((FieldReference)tokenMiniBossLiss).ResolveReflection();
        FieldInfo miniBossCardField = ((FieldReference)tokenMiniBossCard).ResolveReflection();
        FieldInfo specialListField = ((FieldReference)tokenSpecialList).ResolveReflection();

        // The list of bosses game ids at B.O.S.S. Typically matches the enemy id concerned, but not always for special sets of enemies.
        int[] bossList = _assemblyCSharpDataCollector.ReadIntArrayFromPrivateImplementationDetailField(bossListField);
        // The list of mini bosses game ids at B.O.S.S. Typically matches the enemy id concerned, but not always for special sets of enemies.
        int[] miniBossList =
            _assemblyCSharpDataCollector.ReadIntArrayFromPrivateImplementationDetailField(miniBossListField);
        // The list of enemy game ids whose Spy Data are considered as mini bosses and thus, rare.
        // Typically, it means the matching base game Spy Card is a mini boss, but this isn't always true as it notably
        // excludes the DeadLander enemies.
        int[] miniBossCard =
            _assemblyCSharpDataCollector.ReadIntArrayFromPrivateImplementationDetailField(miniBossCardField);
        // The fire and ice variants of the Krawler, Cape and Warden which have some special handling in the game.
        int[] specialList =
            _assemblyCSharpDataCollector.ReadIntArrayFromPrivateImplementationDetailField(specialListField);

        IEnumerable<int> enemyIdsExcludedFromBestiary = Enumerable.Range(0, _orderedRegistry.Registry.Count)
            .Except(_orderedRegistry.BaseGameIdsToOrderingIndex.Keys);

        // The MenderBot is hardcoded in the game to be excluded.
        HashSet<int> excludedEnemyGameIdsFromRandomCot = bossList
            .Concat(miniBossList)
            .Concat(miniBossCard)
            .Concat(enemyIdsExcludedFromBestiary)
            .Except(specialList)
            .Append((int)MainManager.Enemies.MenderBot)
            .Where(e => e >= 0)
            .ToHashSet();

        // The GoldenSeedling is hardcoded to be considered rare.
        HashSet<int> gameIdsWithRareSpyData = bossList
            .Concat(miniBossCard)
            .Append((int)MainManager.Enemies.GoldenSeedling)
            .Where(e => e >= 0)
            .ToHashSet();

        for (int i = 0; i < _enemyNamedIds.Length; i++)
        {
            HasEnemyLeaf hasEnemyLeaf = _orderedRegistry.Registry.GetByGameId(i);
            _enemyTextAssetParser.FromTextAssetSerializedString(
                ResourcesPaths.DataEnemiesPath,
                _enemiesData[i],
                hasEnemyLeaf);
            for (int j = 0; j < RootCollector.LanguageDisplayNames.Length; j++)
            {
                hasEnemyLeaf.LocalizedData[_languageRegistry.GetByGameId(j)] = new();
                _enemyLocalizedTextAssetParser.FromTextAssetSerializedString(
                    ResourcesPaths.DataLocalizedBestiaryEntriesPathSuffix,
                    j,
                    _enemiesLanguageData[j][i],
                    hasEnemyLeaf);
            }

            if (excludedEnemyGameIdsFromRandomCot.Contains(i))
                hasEnemyLeaf.IsIncludedInRandomCaveOfTrialsPool = false;
            if (gameIdsWithRareSpyData.Contains(i))
                hasEnemyLeaf.IsRareSpyData = true;
        }

        foreach (HasEnemyLeaf leaf in _orderedRegistry.Registry)
        {
            IHasEnemyPortraitSprite hasEnemyPortraitSprite = leaf;
            // Enemies' EnemyPortraits sprites indexes are special because any negativew numbers means that the index is
            // equal to the enemy id. We need this knowledge to read the index, but after reindexation by the patcher,
            // these will go away since we want to use our own indexes regardless of the lead type.
            if (hasEnemyPortraitSprite.EnemyPortraitsSpriteIndex < 0)
                hasEnemyPortraitSprite.EnemyPortraitsSpriteIndex = leaf.GameId;

            hasEnemyPortraitSprite.PortraitSprite = new AssetLoaderFromResources<Sprite>(
                ResourcesPaths.SpritesItemsEnemyPortraitsResourcesPath,
                hasEnemyPortraitSprite.EnemyPortraitsSpriteIndex!.Value);
        }

        RootCollector.LogCollectedAmount(_logger, _orderedRegistry.Registry, _enemyNamedIds.Length);
    }
}