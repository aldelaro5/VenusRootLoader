using VenusRootLoader.GameContent;
using VenusRootLoader.Patching;
using VenusRootLoader.Patching.TextAssetData;

namespace VenusRootLoader.Binders;

internal sealed class ItemBinder
{
    private readonly EnumPatcher _enumPatcher;
    private readonly ItemAndMedalSpritePatcher _itemAndMedalSpritePatcher;
    private readonly TextAssetPatcher<ItemData> _itemDataPatcher;
    private readonly LocalizedTextAssetPatcher<ItemLanguageData> _itemLanguageDataPatcher;

    public ItemBinder(
        ItemAndMedalSpritePatcher itemAndMedalSpritePatcher,
        TextAssetPatcher<ItemData> itemDataPatcher,
        LocalizedTextAssetPatcher<ItemLanguageData> itemLanguageDataPatcher,
        EnumPatcher enumPatcher)
    {
        _itemAndMedalSpritePatcher = itemAndMedalSpritePatcher;
        _itemDataPatcher = itemDataPatcher;
        _itemLanguageDataPatcher = itemLanguageDataPatcher;
        _enumPatcher = enumPatcher;
    }

    internal ItemContent BindNewItem(string namedId)
    {
        int newId = _enumPatcher.AddCustomEnumName(typeof(MainManager.Items), namedId);
        ItemContent content = new() { GameId = newId };
        _itemDataPatcher.AddNewDataToTextAsset(content.ItemData);
        _itemLanguageDataPatcher.AddNewDataToTextAsset(content.ItemLanguageData);
        _itemAndMedalSpritePatcher.AssignItemSprite(newId, content.ItemSprite);
        return content;
    }

    internal ItemContent BindExistingItem(int itemId)
    {
        ItemContent content = new() { GameId = itemId };
        _itemDataPatcher.ChangeVanillaDataOfTextAsset(itemId, content.ItemData);
        _itemLanguageDataPatcher.ChangeVanillaDataOfTextAsset(itemId, content.ItemLanguageData);
        _itemAndMedalSpritePatcher.AssignItemSprite(itemId, content.ItemSprite);
        return content;
    }
}