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
    private readonly string _recordsOrderingData =
        RootCollector.ReadWholeTextAsset(TextAssetPaths.DataRecordsOrderingPath);

    private readonly Dictionary<int, string[]> _recordsLanguageData =
        RootCollector.ReadLocalizedTestAssetLines(TextAssetPaths.DataLocalizedRecordsPathSuffix);

    private readonly Sprite[] _enemyPortraitsSprites = Resources.LoadAll<Sprite>(
        $"{TextAssetPaths.RootSpritesPathPrefix}{TextAssetPaths.SpritesEnemyPortraitsPath}");

    private readonly ILogger<RecordsCollector> _logger;
    private readonly IOrderedLeavesRegistry<RecordLeaf> _orderedRegistry;
    private readonly ILeavesRegistry<LanguageLeaf> _languageRegistry;
    private readonly IOrderingTextAssetParser<RecordLeaf> _recordsOrderingDataSerializer;
    private readonly ILocalizedTextAssetParser<RecordLeaf> _recordsLanguageDataSerializer;

    public RecordsCollector(
        ILogger<RecordsCollector> logger,
        IOrderedLeavesRegistry<RecordLeaf> orderedRegistry,
        ILeavesRegistry<LanguageLeaf> languageRegistry,
        IOrderingTextAssetParser<RecordLeaf> recordsOrderingDataSerializer,
        ILocalizedTextAssetParser<RecordLeaf> recordsLanguageDataSerializer)
    {
        _logger = logger;
        _orderedRegistry = orderedRegistry;
        _recordsOrderingDataSerializer = recordsOrderingDataSerializer;
        _recordsLanguageDataSerializer = recordsLanguageDataSerializer;
        _languageRegistry = languageRegistry;
    }

    public void CollectBaseGameData()
    {
        int recordsAmount = _recordsOrderingData
            .Split('\n')
            .Length;
        for (int i = 0; i < recordsAmount; i++)
        {
            RecordLeaf recordLeaf = _orderedRegistry.RegisterExistingWithOrdering(i, i.ToString());
            for (int j = 0; j < RootCollector.LanguageDisplayNames.Length; j++)
            {
                recordLeaf.LocalizedData[_languageRegistry.GetByGameId(j)] = new();
                _recordsLanguageDataSerializer.FromTextAssetSerializedString(
                    TextAssetPaths.DataLocalizedRecordsPathSuffix,
                    j,
                    _recordsLanguageData[j][i],
                    recordLeaf);
            }
        }

        _recordsOrderingDataSerializer.FromTextAssetString(_recordsOrderingData, _orderedRegistry);
        foreach (RecordLeaf leaf in _orderedRegistry.Registry)
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