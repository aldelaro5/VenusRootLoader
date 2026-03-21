using Microsoft.Extensions.Logging;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Patching.Resources.TextAssetPatchers;
using VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;
using VenusRootLoader.Registry;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class DiscoveriesCollector : IBaseGameCollector
{
    private static readonly string DiscoveriesOrderingData = Resources.Load<TextAsset>("Data/DiscoveryOrder").text
        .Trim('\n');

    private static readonly Dictionary<int, string[]> DiscoveriesLanguageData = new();

    private readonly Sprite[] _enemyPortraitsSprites = Resources.LoadAll<Sprite>("Sprites/Items/EnemyPortraits");

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

        for (int i = 0; i < RootCollector.LanguageDisplayNames.Length; i++)
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
            for (int j = 0; j < RootCollector.LanguageDisplayNames.Length; j++)
            {
                discoveryLeaf.LocalizedData[j] = new();
                _discoveriesLanguageDataSerializer.FromTextAssetSerializedString(
                    "Discoveries",
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