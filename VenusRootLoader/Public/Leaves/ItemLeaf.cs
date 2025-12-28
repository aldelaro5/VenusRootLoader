using Microsoft.Extensions.Logging;
using UnityEngine;
using VenusRootLoader.GameContent;
using VenusRootLoader.Patching.Resources.TextAsset.SerializableData;

namespace VenusRootLoader.Public.Leaves;

public sealed class ItemLeaf : Leaf<int>
{
    private readonly ItemContent _content;

    protected override string ContentTypeName => "Item";
    public override int GameId => _content.GameId;
    public override string NamedId => _content.NamedId;
    public override string CreatorId => _content.CreatorId;

    internal ItemLeaf(ItemContent content, ILogger<Venus> logger)
        : base(logger) => _content = content;

    public ItemLeaf WithBuyingPrice(int buyingPrice)
    {
        _content.ItemData.BuyingPrice = buyingPrice;
        LogChange();
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
        LogChange();
        return this;
    }

    public ItemLeaf WithTarget(BattleControl.AttackArea attackArea)
    {
        _content.ItemData.Target = attackArea;
        LogChange();
        return this;
    }

    public ItemLeaf WithSprite(Sprite sprite)
    {
        _content.ItemSprite.Sprite = sprite;
        LogChange();
        return this;
    }

    public ItemLeaf WithName(int languageId, string name)
    {
        ItemLanguageData itemLanguageData = GetOrCreateLanguageDataForLanguage(languageId, _content.ItemLanguageData);
        itemLanguageData.Name = name;
        LogChange();
        return this;
    }

    public ItemLeaf WithDescription(int languageId, string description)
    {
        ItemLanguageData itemLanguageData = GetOrCreateLanguageDataForLanguage(languageId, _content.ItemLanguageData);
        itemLanguageData.Description = description;
        LogChange();
        return this;
    }

    public ItemLeaf WithPrependerString(int languageId, string prepender)
    {
        ItemLanguageData itemLanguageData = GetOrCreateLanguageDataForLanguage(languageId, _content.ItemLanguageData);
        itemLanguageData.Prepender = prepender;
        LogChange();
        return this;
    }
}