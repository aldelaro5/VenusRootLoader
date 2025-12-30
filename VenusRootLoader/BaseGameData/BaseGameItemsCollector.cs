using Microsoft.Extensions.Logging;
using UnityEngine;
using VenusRootLoader.GameContent;
using VenusRootLoader.VenusInternals;

namespace VenusRootLoader.BaseGameData;

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
    private readonly ContentRegistry _contentRegistry;

    public BaseGameItemsCollector(ContentRegistry contentRegistry, ILogger<BaseGameItemsCollector> logger)
    {
        _contentRegistry = contentRegistry;
        _logger = logger;

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
            ItemContent itemContent = _contentRegistry.RegisterAndBindExistingItem(i, itemNamedId, baseGameId);
            itemContent.ItemData.FromTextAssetSerializedString(ItemsData[i]);
            itemContent.ItemSprite.Sprite = i < ItemsSpritesAmountInItems0
                ? _items0Sprites[i]
                : _items1Sprites[i - ItemsSpritesAmountInItems0];
            for (int j = 0; j < BaseGameDataCollector.LanguageDisplayNames.Length; j++)
            {
                itemContent.ItemLanguageData[j] = new();
                itemContent.ItemLanguageData[j].FromTextAssetSerializedString(ItemsLanguageData[j][i]);
            }
        }

        _logger.LogInformation("Registered {ItemsAmount} base game items", _itemNamedIds.Length);
    }
}