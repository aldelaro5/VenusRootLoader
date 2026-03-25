using Microsoft.Extensions.Logging;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class RecordsCollector : IBaseGameCollector
{
    private static readonly string RecordsOrderingData =
        RootCollector.ReadWholeTextAsset(TextAssetPaths.DataRecordsOrderingPath);

    private static readonly Dictionary<int, string[]> RecordsLanguageData =
        RootCollector.ReadLocalizedTestAssetLines(TextAssetPaths.DataLocalizedRecordsPathSuffix);

    private readonly Sprite[] _enemyPortraitsSprites = Resources.LoadAll<Sprite>(
        $"{TextAssetPaths.RootSpritesPathPrefix}{TextAssetPaths.SpritesEnemyPortraitsPath}");

    private readonly ILogger<RecordsCollector> _logger;
    private readonly IOrderedLeavesRegistry<RecordLeaf> _orderedRegistry;
    private readonly IOrderingTextAssetParser<RecordLeaf> _recordsOrderingDataSerializer;
    private readonly ILocalizedTextAssetParser<RecordLeaf> _recordsLanguageDataSerializer;

    public RecordsCollector(
        ILogger<RecordsCollector> logger,
        IOrderedLeavesRegistry<RecordLeaf> orderedRegistry,
        IOrderingTextAssetParser<RecordLeaf> recordsOrderingDataSerializer,
        ILocalizedTextAssetParser<RecordLeaf> recordsLanguageDataSerializer)
    {
        _logger = logger;
        _orderedRegistry = orderedRegistry;
        _recordsOrderingDataSerializer = recordsOrderingDataSerializer;
        _recordsLanguageDataSerializer = recordsLanguageDataSerializer;
    }

    public void CollectBaseGameData(string baseGameId)
    {
        int recordsAmount = RecordsOrderingData
            .Split('\n')
            .Length;
        for (int i = 0; i < recordsAmount; i++)
        {
            RecordLeaf recordLeaf = _orderedRegistry.RegisterExistingWithOrdering(i, i.ToString(), baseGameId);
            for (int j = 0; j < RootCollector.LanguageDisplayNames.Length; j++)
            {
                recordLeaf.LocalizedData[j] = new();
                _recordsLanguageDataSerializer.FromTextAssetSerializedString(
                    TextAssetPaths.DataLocalizedRecordsPathSuffix,
                    j,
                    RecordsLanguageData[j][i],
                    recordLeaf);
            }
        }

        _recordsOrderingDataSerializer.FromTextAssetString(RecordsOrderingData, _orderedRegistry);
        foreach (RecordLeaf leaf in _orderedRegistry.Registry.LeavesByGameIds.Values)
        {
            IEnemyPortraitSprite enemyPortraitStuff = leaf;
            enemyPortraitStuff.WrappedSprite.Sprite =
                _enemyPortraitsSprites[enemyPortraitStuff.EnemyPortraitsSpriteIndex!.Value];
        }

        _logger.LogInformation(
            "Collected and registered {RcordsAmount} base game records",
            recordsAmount);
    }
}