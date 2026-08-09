using Microsoft.Extensions.Logging;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;
using VenusRootLoader.Registry;
using VenusRootLoader.Unity.AssetLoading;
using VenusRootLoader.Utility;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class RecordsCollector : IBaseGameCollector
{
    private readonly string _recordsOrderingData =
        RootCollector.ReadWholeTextAsset(ResourcesPaths.DataRecordsOrderingPath);

    private readonly Dictionary<int, string[]> _recordsLanguageData =
        RootCollector.ReadLocalizedTestAssetLines(ResourcesPaths.DataLocalizedRecordsPathSuffix);

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
                    ResourcesPaths.DataLocalizedRecordsPathSuffix,
                    j,
                    _recordsLanguageData[j][i],
                    recordLeaf);
            }
        }

        _recordsOrderingDataSerializer.FromTextAssetString(_recordsOrderingData, _orderedRegistry);
        foreach (RecordLeaf leaf in _orderedRegistry.Registry)
        {
            IEnemyPortraitSprite enemyPortraitSprite = leaf;
            enemyPortraitSprite.PortraitSprite = new AssetLoaderFromResources<Sprite>(
                ResourcesPaths.SpritesItemsEnemyPortraitsResourcesPath,
                enemyPortraitSprite.EnemyPortraitsSpriteIndex!.Value);
        }

        RootCollector.LogCollectedAmount(_logger, _orderedRegistry.Registry, recordsAmount);
    }
}