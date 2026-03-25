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
using VenusRootLoader.Utility;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class EnemiesCollector : IBaseGameCollector
{
    private static readonly string[] EnemiesData = RootCollector.ReadTextAssetLines(TextAssetPaths.DataEnemiesPath);

    private static readonly string EnemiesOrderingData =
        RootCollector.ReadWholeTextAsset(TextAssetPaths.DataBestiaryEntriesOrderingPath);

    private static readonly Dictionary<int, string[]> EnemiesLanguageData =
        RootCollector.ReadLocalizedTestAssetLines(TextAssetPaths.DataLocalizedBestiaryEntriesPathSuffix);

    private readonly string[] _enemyNamedIds = Enum.GetNames(typeof(MainManager.Enemies)).ToArray();

    private readonly Sprite[] _enemyPortraitsSprites = Resources.LoadAll<Sprite>(
        $"{TextAssetPaths.RootSpritesPathPrefix}{TextAssetPaths.SpritesEnemyPortraitsPath}");

    private readonly ILogger<EnemiesCollector> _logger;
    private readonly IAssemblyCSharpDataCollector _assemblyCSharpDataCollector;
    private readonly ITextAssetParser<EnemyLeaf> _enemyTextAssetParser;
    private readonly IOrderedLeavesRegistry<EnemyLeaf> _orderedRegistry;
    private readonly IOrderingTextAssetParser<EnemyLeaf> _enemyOrderingTextAssetParser;
    private readonly ILocalizedTextAssetParser<EnemyLeaf> _enemyLocalizedTextAssetParser;

    public EnemiesCollector(
        ILogger<EnemiesCollector> logger,
        IAssemblyCSharpDataCollector assemblyCSharpDataCollector,
        ITextAssetParser<EnemyLeaf> enemyTextAssetParser,
        IOrderedLeavesRegistry<EnemyLeaf> orderedRegistry,
        IOrderingTextAssetParser<EnemyLeaf> enemyOrderingTextAssetParser,
        ILocalizedTextAssetParser<EnemyLeaf> enemyLocalizedTextAssetParser)
    {
        _logger = logger;
        _assemblyCSharpDataCollector = assemblyCSharpDataCollector;
        _orderedRegistry = orderedRegistry;
        _enemyTextAssetParser = enemyTextAssetParser;
        _enemyOrderingTextAssetParser = enemyOrderingTextAssetParser;
        _enemyLocalizedTextAssetParser = enemyLocalizedTextAssetParser;
    }

    public void CollectBaseGameData(string baseGameId)
    {
        _enemyOrderingTextAssetParser.FromTextAssetString(EnemiesOrderingData, _orderedRegistry);

        for (int i = 0; i < _enemyNamedIds.Length; i++)
        {
            string enemyNamedId = _enemyNamedIds[i];

            if (_orderedRegistry.BaseGameIdsToOrderingIndex.ContainsKey(i))
                _orderedRegistry.RegisterExistingWithOrdering(i, enemyNamedId, baseGameId);
            else
                _orderedRegistry.Registry.RegisterExisting(i, enemyNamedId, baseGameId);
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
        FieldInfo excludeIdsField = ((FieldReference)tokenSpecialList).ResolveReflection();

        int[] bossList = _assemblyCSharpDataCollector.ReadIntArrayFromPrivateImplementationDetailField(bossListField);
        int[] miniBossList =
            _assemblyCSharpDataCollector.ReadIntArrayFromPrivateImplementationDetailField(miniBossListField);
        int[] miniBossCard =
            _assemblyCSharpDataCollector.ReadIntArrayFromPrivateImplementationDetailField(miniBossCardField);
        int[] specialList =
            _assemblyCSharpDataCollector.ReadIntArrayFromPrivateImplementationDetailField(excludeIdsField);

        IEnumerable<int> enemyIdsExcludedFromBestiary = _orderedRegistry.Registry.LeavesByGameIds.Keys
            .Except(_orderedRegistry.BaseGameIdsToOrderingIndex.Keys);
        HashSet<int> excludedEnemyGameIdsFromRandomCot = bossList
            .Concat(miniBossList)
            .Concat(miniBossCard)
            .Concat(enemyIdsExcludedFromBestiary)
            .Except(specialList)
            .Append((int)MainManager.Enemies.MenderBot)
            .Where(e => e >= 0)
            .ToHashSet();

        HashSet<int> gameIdsWithRareSpyData = bossList
            .Concat(miniBossCard)
            .Append((int)MainManager.Enemies.GoldenSeedling)
            .Where(e => e >= 0)
            .ToHashSet();

        for (int i = 0; i < _enemyNamedIds.Length; i++)
        {
            EnemyLeaf enemyLeaf = _orderedRegistry.Registry.LeavesByGameIds[i];
            _enemyTextAssetParser.FromTextAssetSerializedString(
                TextAssetPaths.DataEnemiesPath,
                EnemiesData[i],
                enemyLeaf);
            for (int j = 0; j < RootCollector.LanguageDisplayNames.Length; j++)
            {
                enemyLeaf.LocalizedData[j] = new();
                _enemyLocalizedTextAssetParser.FromTextAssetSerializedString(
                    TextAssetPaths.DataLocalizedBestiaryEntriesPathSuffix,
                    j,
                    EnemiesLanguageData[j][i],
                    enemyLeaf);
            }

            if (excludedEnemyGameIdsFromRandomCot.Contains(i))
                enemyLeaf.IsIncludedInRandomCaveOfTrialsPool = false;
            if (gameIdsWithRareSpyData.Contains(i))
                enemyLeaf.IsRareSpyData = true;
        }

        foreach (EnemyLeaf leaf in _orderedRegistry.Registry.LeavesByGameIds.Values)
        {
            IEnemyPortraitSprite enemyPortraitSprite = leaf;
            if (enemyPortraitSprite.EnemyPortraitsSpriteIndex < 0)
                enemyPortraitSprite.EnemyPortraitsSpriteIndex = leaf.GameId;

            enemyPortraitSprite.WrappedSprite.Sprite =
                _enemyPortraitsSprites[enemyPortraitSprite.EnemyPortraitsSpriteIndex!.Value];
        }

        _logger.LogInformation("Collected and registered {EnemiesAmount} base game enemies", _enemyNamedIds.Length);
    }
}