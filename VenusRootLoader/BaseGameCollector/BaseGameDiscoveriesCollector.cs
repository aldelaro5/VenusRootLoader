using Microsoft.Extensions.Logging;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Registry;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class BaseGameDiscoveriesCollector : IBaseGameCollector
{
    private static readonly string DiscoveriesOrderingData = Resources.Load<TextAsset>("Data/DiscoveryOrder").text
        .Trim('\n');

    private static readonly Dictionary<int, string[]> DiscoveriesLanguageData = new();

    private readonly Sprite[] _enemyPortraitsSprites = Resources.LoadAll<Sprite>("Sprites/Items/EnemyPortraits");

    private readonly ILogger<BaseGameDiscoveriesCollector> _logger;
    private readonly IOrderedLeavesRegistry<DiscoveryLeaf> _orderedRegistry;
    private readonly IOrderingTextAssetParser<DiscoveryLeaf> _discoveriesOrderingDataSerializer;
    private readonly ILocalizedTextAssetParser<DiscoveryLeaf> _discoveriesLanguageDataSerializer;

    public BaseGameDiscoveriesCollector(
        IOrderedLeavesRegistry<DiscoveryLeaf> orderedRegistry,
        ILogger<BaseGameDiscoveriesCollector> logger,
        IOrderingTextAssetParser<DiscoveryLeaf> discoveriesOrderingDataSerializer,
        ILocalizedTextAssetParser<DiscoveryLeaf> discoveriesLanguageDataSerializer)
    {
        _orderedRegistry = orderedRegistry;
        _logger = logger;
        _discoveriesOrderingDataSerializer = discoveriesOrderingDataSerializer;
        _discoveriesLanguageDataSerializer = discoveriesLanguageDataSerializer;

        for (int i = 0; i < RootBaseGameDataCollector.LanguageDisplayNames.Length; i++)
        {
            string[] discoveryLanguageData = Resources.Load<TextAsset>($"Data/Dialogues{i}/Discoveries").text
                .Trim('\n')
                .Split(['\n'], StringSplitOptions.RemoveEmptyEntries);
            DiscoveriesLanguageData.Add(i, discoveryLanguageData);
        }
    }

    public void CollectBaseGameData(string baseGameId)
    {
        int discoveriesAmount = DiscoveriesOrderingData
            .Split('\n')
            .Length;
        for (int i = 0; i < discoveriesAmount; i++)
        {
            DiscoveryLeaf discoveryLeaf = _orderedRegistry.RegisterExistingWithOrdering(i, i.ToString(), baseGameId);
            for (int j = 0; j < RootBaseGameDataCollector.LanguageDisplayNames.Length; j++)
            {
                discoveryLeaf.LanguageData[j] = new();
                _discoveriesLanguageDataSerializer.FromTextAssetSerializedString(
                    "Discoveries",
                    j,
                    DiscoveriesLanguageData[j][i],
                    discoveryLeaf);
            }
        }

        _discoveriesOrderingDataSerializer.FromTextAssetString(DiscoveriesOrderingData, _orderedRegistry);
        foreach (DiscoveryLeaf leaf in _orderedRegistry.Registry.LeavesByGameIds.Values)
            leaf.WrappedSprite.Sprite = _enemyPortraitsSprites[leaf.EnemyPortraitsSpriteIndex!.Value];

        _logger.LogInformation(
            "Collected and registered {DiscoveriesAmount} base game discoveries",
            discoveriesAmount);
    }
}