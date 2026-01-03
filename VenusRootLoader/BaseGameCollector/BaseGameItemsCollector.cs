using Microsoft.Extensions.Logging;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Registry;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class BaseGameItemsCollector : IBaseGameCollector
{
    private const int ItemsSpritesAmountInItems0 = 176;

    private static readonly string[] ItemsData = Resources.Load<TextAsset>("Data/ItemData").text
        .Trim('\n')
        .Split(['\n'], StringSplitOptions.RemoveEmptyEntries);

    private static readonly Dictionary<int, string[]> ItemsLanguageData = new();

    private readonly string[] _itemNamedIds = Enum.GetNames(typeof(MainManager.Items))
        .TakeWhile(v => v != nameof(MainManager.Items.None))
        .ToArray();

    private readonly Sprite[] _items0Sprites = Resources.LoadAll<Sprite>("Sprites/Items/Items0");
    private readonly Sprite[] _items1Sprites = Resources.LoadAll<Sprite>("Sprites/Items/Items1");

    private readonly ILogger<BaseGameItemsCollector> _logger;
    private readonly ITextAssetSerializable<ItemLeaf> _itemDataSerializer;
    private readonly ILocalizedTextAssetSerializable<ItemLeaf> _itemLanguageDataSerializer;
    private readonly ILeavesRegistry<ItemLeaf> _leavesRegistry;

    public BaseGameItemsCollector(
        ILeavesRegistry<ItemLeaf> leavesRegistry,
        ILogger<BaseGameItemsCollector> logger,
        ITextAssetSerializable<ItemLeaf> itemDataSerializer,
        ILocalizedTextAssetSerializable<ItemLeaf> itemLanguageDataSerializer)
    {
        _leavesRegistry = leavesRegistry;
        _logger = logger;
        _itemDataSerializer = itemDataSerializer;
        _itemLanguageDataSerializer = itemLanguageDataSerializer;

        for (int i = 0; i < RootBaseGameDataCollector.LanguageDisplayNames.Length; i++)
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

    public void CollectBaseGameData(string baseGameId)
    {
        for (int i = 0; i < _itemNamedIds.Length; i++)
        {
            string itemNamedId = _itemNamedIds[i];
            ItemLeaf itemLeaf = _leavesRegistry.RegisterExisting(i, itemNamedId, baseGameId);
            _itemDataSerializer.FromTextAssetSerializedString(ItemsData[i], itemLeaf);
            itemLeaf.WrappedSprite.Sprite = i < ItemsSpritesAmountInItems0
                ? _items0Sprites[i]
                : _items1Sprites[i - ItemsSpritesAmountInItems0];
            for (int j = 0; j < RootBaseGameDataCollector.LanguageDisplayNames.Length; j++)
            {
                itemLeaf.LanguageData[j] = new();
                _itemLanguageDataSerializer.FromTextAssetSerializedString(
                    j,
                    ItemsLanguageData[j][i],
                    itemLeaf);
            }
        }

        _logger.LogInformation("Collected and registered {ItemsAmount} base game items", _itemNamedIds.Length);
    }
}