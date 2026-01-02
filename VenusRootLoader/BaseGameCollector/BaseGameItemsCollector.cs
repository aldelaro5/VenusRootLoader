using Microsoft.Extensions.Logging;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.VenusInternals;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class BaseGameItemsCollector
{
    private const int ItemsSpritesAmountInItems0 = 176;

    internal static readonly string[] ItemsData = Resources.Load<TextAsset>("Data/ItemData").text
        .Trim('\n')
        .Split(['\n'], StringSplitOptions.RemoveEmptyEntries);

    internal static readonly Dictionary<int, string[]> ItemsLanguageData = new();

    private readonly string[] _itemNamedIds = Enum.GetNames(typeof(MainManager.Items))
        .TakeWhile(v => v != nameof(MainManager.Items.None))
        .ToArray();

    private readonly Sprite[] _items0Sprites = Resources.LoadAll<Sprite>("Sprites/Items/Items0");
    private readonly Sprite[] _items1Sprites = Resources.LoadAll<Sprite>("Sprites/Items/Items1");

    private readonly ILogger<BaseGameItemsCollector> _logger;
    private readonly ITextAssetSerializable<ItemLeaf> _itemDataSerializer;
    private readonly ITextAssetSerializable<ItemLeaf.ItemLanguageData> _itemLanguageDataSerializer;
    private readonly LeavesRegistry _leavesRegistry;

    public BaseGameItemsCollector(
        LeavesRegistry leavesRegistry,
        ILogger<BaseGameItemsCollector> logger,
        ITextAssetSerializable<ItemLeaf> itemDataSerializer,
        ITextAssetSerializable<ItemLeaf.ItemLanguageData> itemLanguageDataSerializer)
    {
        _leavesRegistry = leavesRegistry;
        _logger = logger;
        _itemDataSerializer = itemDataSerializer;
        _itemLanguageDataSerializer = itemLanguageDataSerializer;

        for (int i = 0; i < BaseGameDataCollector.LanguageDisplayNames.Length; i++)
        {
            string[] itemLanguageData = Resources.Load<TextAsset>($"Data/Dialogues{i}/Items").text
                .Trim('\n')
                .Split(['\n'], StringSplitOptions.RemoveEmptyEntries);
            // Workaround a game bug where not all languages has the last line about BigBerry
            if (itemLanguageData.Length != _itemNamedIds.Length)
                itemLanguageData = itemLanguageData.Append("RESERVED@Desc@Desc@a").ToArray();
            ItemsLanguageData.Add(i, itemLanguageData);
        }
    }

    internal void CollectAndRegisterItems(string baseGameId)
    {
        for (int i = 0; i < _itemNamedIds.Length; i++)
        {
            string itemNamedId = _itemNamedIds[i];
            ItemLeaf itemLeaf = _leavesRegistry.RegisterAndBindExistingItem(i, itemNamedId, baseGameId);
            _itemDataSerializer.FromTextAssetSerializedString(ItemsData[i], itemLeaf);
            itemLeaf.WrappedSprite.Sprite = i < ItemsSpritesAmountInItems0
                ? _items0Sprites[i]
                : _items1Sprites[i - ItemsSpritesAmountInItems0];
            for (int j = 0; j < BaseGameDataCollector.LanguageDisplayNames.Length; j++)
            {
                itemLeaf.LanguageData[j] = new();
                _itemLanguageDataSerializer.FromTextAssetSerializedString(
                    ItemsLanguageData[j][i],
                    itemLeaf.LanguageData[j]);
            }
        }

        _logger.LogInformation("Registered {ItemsAmount} base game items", _itemNamedIds.Length);
    }
}