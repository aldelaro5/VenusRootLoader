using Microsoft.Extensions.Logging;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class DiscoveriesCollector : IBaseGameCollector
{
    private readonly string _discoveriesOrderingData =
        RootCollector.ReadWholeTextAsset(TextAssetPaths.DataDiscoveriesOrderingPath);

    private readonly Dictionary<int, string[]> _discoveriesLanguageData =
        RootCollector.ReadLocalizedTestAssetLines(TextAssetPaths.DataLocalizedDiscoveriesPathSuffix);

    private readonly Sprite[] _enemyPortraitsSprites = Resources.LoadAll<Sprite>(
        $"{TextAssetPaths.RootSpritesPathPrefix}{TextAssetPaths.SpritesEnemyPortraitsPath}");

    private readonly ILogger<DiscoveriesCollector> _logger;
    private readonly IOrderedLeavesRegistry<DiscoveryLeaf> _orderedRegistry;
    private readonly IOrderingTextAssetParser<DiscoveryLeaf> _discoveriesOrderingDataSerializer;
    private readonly ILocalizedTextAssetParser<DiscoveryLeaf> _discoveriesLanguageDataSerializer;

    public DiscoveriesCollector(
        IOrderedLeavesRegistry<DiscoveryLeaf> orderedRegistry,
        ILogger<DiscoveriesCollector> logger,
        IOrderingTextAssetParser<DiscoveryLeaf> discoveriesOrderingDataSerializer,
        ILocalizedTextAssetParser<DiscoveryLeaf> discoveriesLanguageDataSerializer)
    {
        _orderedRegistry = orderedRegistry;
        _logger = logger;
        _discoveriesOrderingDataSerializer = discoveriesOrderingDataSerializer;
        _discoveriesLanguageDataSerializer = discoveriesLanguageDataSerializer;
    }

    public void CollectBaseGameData(string baseGameId)
    {
        int discoveriesAmount = _discoveriesOrderingData
            .Split('\n')
            .Length;
        for (int i = 0; i < discoveriesAmount; i++)
        {
            DiscoveryLeaf discoveryLeaf = _orderedRegistry.RegisterExistingWithOrdering(i, i.ToString(), baseGameId);
            for (int j = 0; j < RootCollector.LanguageDisplayNames.Length; j++)
            {
                discoveryLeaf.LocalizedData[j] = new();
                _discoveriesLanguageDataSerializer.FromTextAssetSerializedString(
                    TextAssetPaths.DataLocalizedDiscoveriesPathSuffix,
                    j,
                    _discoveriesLanguageData[j][i],
                    discoveryLeaf);
            }
        }

        _discoveriesOrderingDataSerializer.FromTextAssetString(_discoveriesOrderingData, _orderedRegistry);
        foreach (DiscoveryLeaf leaf in _orderedRegistry.Registry.LeavesByGameIds.Values)
        {
            IEnemyPortraitSprite enemyPortraitSprite = leaf;
            enemyPortraitSprite.WrappedSprite.Sprite =
                _enemyPortraitsSprites[enemyPortraitSprite.EnemyPortraitsSpriteIndex!.Value];
        }

        _logger.LogInformation(
            "Collected and registered {DiscoveriesAmount} base game discoveries",
            discoveriesAmount);
    }
}