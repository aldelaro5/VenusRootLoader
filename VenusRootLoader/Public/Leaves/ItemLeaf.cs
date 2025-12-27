using UnityEngine;
using VenusRootLoader.GameContent;
using VenusRootLoader.Internal;
using VenusRootLoader.Patching.Resources.TextAsset.SerializableData;

namespace VenusRootLoader.Public.Leaves;

public sealed class ItemLeaf : ILeaf<int>
{
    private readonly ItemContent _content;

    public int GameId => _content.GameId;
    public string NamedId { get; internal init; }
    public string CreatorId { get; internal init; }
    public string OwnerId { get; internal init; }

    internal ItemLeaf(ItemContent content, string namedId, string creatorId, string ownerId)
    {
        _content = content;
        CreatorId = creatorId;
        OwnerId = ownerId;
        NamedId = namedId;
    }

    public ItemLeaf WithBuyingPrice(int buyingPrice)
    {
        _content.ItemData.BuyingPrice = buyingPrice;
        return this;
    }

    public ItemLeaf WithEffect(MainManager.ItemUsage itemUsage, int value)
    {
        _content.ItemData.Effects.Add(
            new()
            {
                UseType = itemUsage,
                Value = value
            });
        return this;
    }

    public ItemLeaf WithTarget(BattleControl.AttackArea attackArea)
    {
        _content.ItemData.Target = attackArea;
        return this;
    }

    public ItemLeaf WithSprite(Sprite sprite)
    {
        _content.ItemSprite.Sprite = sprite;
        return this;
    }

    public ItemLeaf WithName(int languageId, string name)
    {
        ItemLanguageData itemLanguageData = GetOrCreateLanguageDataForLanguage(languageId);
        itemLanguageData.Name = name;
        return this;
    }

    public ItemLeaf WithDescription(int languageId, string description)
    {
        ItemLanguageData itemLanguageData = GetOrCreateLanguageDataForLanguage(languageId);
        itemLanguageData.Description = description;
        return this;
    }

    public ItemLeaf WithPrependerString(int languageId, string prepender)
    {
        ItemLanguageData itemLanguageData = GetOrCreateLanguageDataForLanguage(languageId);
        itemLanguageData.Prepender = prepender;
        return this;
    }

    private ItemLanguageData GetOrCreateLanguageDataForLanguage(int languageId)
    {
        if (_content.ItemLanguageData.TryGetValue(languageId, out ItemLanguageData? itemLanguageData))
            return itemLanguageData;

        itemLanguageData = new();
        _content.ItemLanguageData[languageId] = itemLanguageData;
        return itemLanguageData;
    }
}