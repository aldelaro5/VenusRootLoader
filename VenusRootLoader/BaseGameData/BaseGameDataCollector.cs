using UnityEngine;
using VenusRootLoader.GameContent;
using VenusRootLoader.VenusInternals;

namespace VenusRootLoader.BaseGameData;

internal sealed class BaseGameDataCollector
{
    private const int ItemsSpritesAmountInItems0 = 176;

    internal static readonly string[] ItemsData = Resources.Load<TextAsset>("Data/ItemData").text
        .Trim('\n')
        .Split(['\n'], StringSplitOptions.RemoveEmptyEntries);

    internal static readonly Dictionary<int, string[]> ItemsLanguageData = new();

    private readonly string[] _languageDisplayNames = MainManager.languagenames.ToArray();

    private readonly string[] _itemNamedIds = Enum.GetNames(typeof(MainManager.Items))
        .TakeWhile(v => v != nameof(MainManager.Items.None))
        .ToArray();

    private readonly Sprite[] _items0Sprites = Resources.LoadAll<Sprite>("Sprites/Items/Items0");
    private readonly Sprite[] _items1Sprites = Resources.LoadAll<Sprite>("Sprites/Items/Items1");

    private readonly ContentRegistry _contentRegistry;

    public BaseGameDataCollector(ContentRegistry contentRegistry)
    {
        _contentRegistry = contentRegistry;
        InitializeItemsLanguageData();
    }

    private void InitializeItemsLanguageData()
    {
        for (int i = 0; i < _languageDisplayNames.Length; i++)
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

    internal void CollectAndRegisterBaseGameData(string baseGameId)
    {
        CollectAndRegisterItems(baseGameId);
    }

    private void CollectAndRegisterItems(string baseGameId)
    {
        for (int i = 0; i < _itemNamedIds.Length; i++)
        {
            string itemNamedId = _itemNamedIds[i];
            ItemContent itemContent = _contentRegistry.RegisterAndBindExistingItem(i, itemNamedId, baseGameId);
            itemContent.ItemData.FromTextAssetSerializedString(ItemsData[i]);
            itemContent.ItemSprite.Sprite = i < ItemsSpritesAmountInItems0
                ? _items0Sprites[i]
                : _items1Sprites[i - ItemsSpritesAmountInItems0];
            for (int j = 0; j < _languageDisplayNames.Length; j++)
            {
                itemContent.ItemLanguageData[j] = new();
                itemContent.ItemLanguageData[j].FromTextAssetSerializedString(ItemsLanguageData[j][i]);
            }
        }
    }
}