using Microsoft.Extensions.Logging;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Registry;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class BaseGameRecordsCollector : IBaseGameCollector
{
    private static readonly string RecordsOrderingData = Resources.Load<TextAsset>("Data/SynopsisOrder").text
        .Trim('\n');

    private static readonly Dictionary<int, string[]> RecordsLanguageData = new();

    private readonly Sprite[] _enemyPortraitsSprites = Resources.LoadAll<Sprite>("Sprites/Items/EnemyPortraits");

    private readonly ILogger<BaseGameRecordsCollector> _logger;
    private readonly IOrderedLeavesRegistry<RecordLeaf> _orderedRegistry;
    private readonly IOrderingTextAssetParser<RecordLeaf> _recordsOrderingDataSerializer;
    private readonly ILocalizedTextAssetParser<RecordLeaf> _recordsLanguageDataSerializer;

    public BaseGameRecordsCollector(
        ILogger<BaseGameRecordsCollector> logger,
        IOrderedLeavesRegistry<RecordLeaf> orderedRegistry,
        IOrderingTextAssetParser<RecordLeaf> recordsOrderingDataSerializer,
        ILocalizedTextAssetParser<RecordLeaf> recordsLanguageDataSerializer)
    {
        _logger = logger;
        _orderedRegistry = orderedRegistry;
        _recordsOrderingDataSerializer = recordsOrderingDataSerializer;
        _recordsLanguageDataSerializer = recordsLanguageDataSerializer;

        for (int i = 0; i < RootBaseGameDataCollector.LanguageDisplayNames.Length; i++)
        {
            string[] recordsLanguageData = Resources.Load<TextAsset>($"Data/Dialogues{i}/Synopsis").text
                .Trim('\n')
                .Split(['\n'], StringSplitOptions.RemoveEmptyEntries);
            RecordsLanguageData.Add(i, recordsLanguageData);
        }
    }

    public void CollectBaseGameData(string baseGameId)
    {
        int recordsAmount = RecordsOrderingData
            .Split('\n')
            .Length;
        for (int i = 0; i < recordsAmount; i++)
        {
            RecordLeaf recordLeaf = _orderedRegistry.RegisterExistingWithOrdering(i, i.ToString(), baseGameId);
            for (int j = 0; j < RootBaseGameDataCollector.LanguageDisplayNames.Length; j++)
            {
                recordLeaf.LocalizedData[j] = new();
                _recordsLanguageDataSerializer.FromTextAssetSerializedString(
                    "Synopsis",
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