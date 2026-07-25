using Microsoft.Extensions.Logging;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class ItemsCollector : IBaseGameCollector
{
    // Items0 contains both items and medals sprites, but they're both contiguous in the array and the game hardcodes the
    // index that separates these 2 regions so we need to hardcode this too.
    private const int ItemsSpritesAmountInItems0 = 176;

    private readonly string[] _itemsData = RootCollector.ReadTextAssetLines(TextAssetPaths.DataItemsPath);

    private readonly Dictionary<int, string[]> _itemsLanguageData =
        RootCollector.ReadLocalizedTestAssetLines(TextAssetPaths.DataLocalizedItemsPathSuffix);

    private readonly string[] _itemNamedIds = Enum.GetNames(typeof(MainManager.Items))
        .TakeWhile(v => v != nameof(MainManager.Items.None))
        .ToArray();

    private readonly Sprite[] _items0Sprites =
        Resources.LoadAll<Sprite>($"{TextAssetPaths.RootSpritesPathPrefix}{TextAssetPaths.SpritesItems0Path}");

    private readonly Sprite[] _items1Sprites =
        Resources.LoadAll<Sprite>($"{TextAssetPaths.RootSpritesPathPrefix}{TextAssetPaths.SpritesItems1Path}");

    private readonly ILogger<ItemsCollector> _logger;
    private readonly ITextAssetParser<ItemLeaf> _itemDataSerializer;
    private readonly ILocalizedTextAssetParser<ItemLeaf> _itemLanguageDataSerializer;
    private readonly ILeavesRegistry<ItemLeaf> _leavesRegistry;

    public ItemsCollector(
        ILeavesRegistry<ItemLeaf> leavesRegistry,
        ILogger<ItemsCollector> logger,
        ITextAssetParser<ItemLeaf> itemDataSerializer,
        ILocalizedTextAssetParser<ItemLeaf> itemLanguageDataSerializer)
    {
        _leavesRegistry = leavesRegistry;
        _logger = logger;
        _itemDataSerializer = itemDataSerializer;
        _itemLanguageDataSerializer = itemLanguageDataSerializer;

        // Workaround a game bug where not all languages has the last line about BigBerry
        for (int i = 0; i < _itemsLanguageData.Count; i++)
        {
            string[] itemLanguageData = _itemsLanguageData[i];
            if (itemLanguageData.Length != _itemNamedIds.Length)
                itemLanguageData = itemLanguageData.Append("RESERVED@Desc@Desc@a").ToArray();
            _itemsLanguageData[i] = itemLanguageData;
        }
    }

    public void CollectBaseGameData(string baseGameId)
    {
        for (int i = 0; i < _itemNamedIds.Length; i++)
        {
            string itemNamedId = _itemNamedIds[i];
            ItemLeaf itemLeaf = _leavesRegistry.RegisterExisting(i, itemNamedId, baseGameId);
            _itemDataSerializer.FromTextAssetSerializedString(TextAssetPaths.DataItemsPath, _itemsData[i], itemLeaf);
            itemLeaf.WrappedSprite.Sprite = i < ItemsSpritesAmountInItems0
                ? _items0Sprites[i]
                : _items1Sprites[i - ItemsSpritesAmountInItems0];
            for (int j = 0; j < RootCollector.LanguageDisplayNames.Length; j++)
            {
                itemLeaf.LocalizedData[j] = new();
                _itemLanguageDataSerializer.FromTextAssetSerializedString(
                    TextAssetPaths.DataLocalizedItemsPathSuffix,
                    j,
                    _itemsLanguageData[j][i],
                    itemLeaf);
            }
        }

        _logger.LogInformation("Collected and registered {ItemsAmount} base game items", _itemNamedIds.Length);
    }
}