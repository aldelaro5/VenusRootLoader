using Microsoft.Extensions.Logging;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;
using VenusRootLoader.Registry;
using VenusRootLoader.Unity.AssetLoading;
using VenusRootLoader.Utility;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class DiscoveriesCollector : IBaseGameCollector
{
    private readonly string _discoveriesOrderingData =
        RootCollector.ReadWholeTextAsset(ResourcesPaths.DataDiscoveriesOrderingPath);

    private readonly Dictionary<int, string[]> _discoveriesLanguageData =
        RootCollector.ReadLocalizedTestAssetLines(ResourcesPaths.DataLocalizedDiscoveriesPathSuffix);

    private readonly ILogger<DiscoveriesCollector> _logger;
    private readonly IOrderedLeavesRegistry<DiscoveryLeaf> _orderedRegistry;
    private readonly ILeavesRegistry<LanguageLeaf> _languageRegistry;
    private readonly IOrderingTextAssetParser<DiscoveryLeaf> _discoveriesOrderingDataSerializer;
    private readonly ILocalizedTextAssetParser<DiscoveryLeaf> _discoveriesLanguageDataSerializer;

    public DiscoveriesCollector(
        IOrderedLeavesRegistry<DiscoveryLeaf> orderedRegistry,
        ILogger<DiscoveriesCollector> logger,
        ILeavesRegistry<LanguageLeaf> languageRegistry,
        IOrderingTextAssetParser<DiscoveryLeaf> discoveriesOrderingDataSerializer,
        ILocalizedTextAssetParser<DiscoveryLeaf> discoveriesLanguageDataSerializer)
    {
        _orderedRegistry = orderedRegistry;
        _logger = logger;
        _languageRegistry = languageRegistry;
        _discoveriesOrderingDataSerializer = discoveriesOrderingDataSerializer;
        _discoveriesLanguageDataSerializer = discoveriesLanguageDataSerializer;
    }

    public void CollectBaseGameData()
    {
        int discoveriesAmount = _discoveriesOrderingData
            .Split('\n')
            .Length;
        for (int i = 0; i < discoveriesAmount; i++)
        {
            DiscoveryLeaf discoveryLeaf = _orderedRegistry.RegisterExistingWithOrdering(i, i.ToString());
            for (int j = 0; j < RootCollector.LanguageDisplayNames.Length; j++)
            {
                discoveryLeaf.LocalizedData[_languageRegistry.GetByGameId(j)] = new();
                _discoveriesLanguageDataSerializer.FromTextAssetSerializedString(
                    ResourcesPaths.DataLocalizedDiscoveriesPathSuffix,
                    j,
                    _discoveriesLanguageData[j][i],
                    discoveryLeaf);
            }
        }

        _discoveriesOrderingDataSerializer.FromTextAssetString(_discoveriesOrderingData, _orderedRegistry);
        foreach (DiscoveryLeaf leaf in _orderedRegistry.Registry)
        {
            IHasEnemyPortraitSprite hasEnemyPortraitSprite = leaf;
            hasEnemyPortraitSprite.PortraitSprite = new AssetLoaderFromResources<Sprite>(
                ResourcesPaths.SpritesItemsEnemyPortraitsResourcesPath,
                hasEnemyPortraitSprite.EnemyPortraitsSpriteIndex!.Value);
        }

        RootCollector.LogCollectedAmount(_logger, _orderedRegistry.Registry, discoveriesAmount);
    }
}