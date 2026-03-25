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
    private static readonly string DiscoveriesOrderingData =
        RootCollector.ReadWholeTextAsset(TextAssetPaths.DataDiscoveriesOrderingPath);

    private static readonly Dictionary<int, string[]> DiscoveriesLanguageData =
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
        int discoveriesAmount = DiscoveriesOrderingData
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
                    DiscoveriesLanguageData[j][i],
                    discoveryLeaf);
            }
        }

        _discoveriesOrderingDataSerializer.FromTextAssetString(DiscoveriesOrderingData, _orderedRegistry);
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