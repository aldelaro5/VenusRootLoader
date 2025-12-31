using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Api.TextAssetData.Items;
using VenusRootLoader.Patching;
using VenusRootLoader.Patching.Resources.Sprite;
using VenusRootLoader.Patching.Resources.TextAsset;

namespace VenusRootLoader.LeafBinding;

internal sealed class ItemLeafBinder : ILeafBinder<ItemLeaf, int>
{
    private readonly EnumPatcher _enumPatcher;
    private readonly ItemAndMedalSpritePatcher _itemAndMedalSpritePatcher;
    private readonly TextAssetPatcher<ItemData> _itemDataPatcher;
    private readonly LocalizedTextAssetPatcher<ItemLanguageData> _itemLanguageDataPatcher;

    public ItemLeafBinder(
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

    public ItemLeaf BindNew(string namedId, string creatorId)
    {
        int newId = _enumPatcher.AddCustomEnumName(typeof(MainManager.Items), namedId);
        ItemLeaf leaf = new()
        {
            GameId = newId,
            CreatorId = creatorId,
            NamedId = namedId
        };
        _itemDataPatcher.AddNewDataToTextAsset(leaf.ItemData);
        _itemLanguageDataPatcher.AddNewDataToTextAsset(leaf.ItemLanguageData);
        _itemAndMedalSpritePatcher.AssignItemSprite(newId, leaf.ItemSprite);
        return leaf;
    }

    public ItemLeaf BindExisting(int itemId, string namedId, string creatorId)
    {
        ItemLeaf leaf = new()
        {
            GameId = itemId,
            NamedId = namedId,
            CreatorId = creatorId
        };
        _itemDataPatcher.ChangeVanillaDataOfTextAsset(itemId, leaf.ItemData);
        _itemLanguageDataPatcher.ChangeVanillaDataOfTextAsset(itemId, leaf.ItemLanguageData);
        _itemAndMedalSpritePatcher.AssignItemSprite(itemId, leaf.ItemSprite);
        return leaf;
    }
}