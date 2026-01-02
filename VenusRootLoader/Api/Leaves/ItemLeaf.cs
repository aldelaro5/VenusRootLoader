using UnityEngine;
using VenusRootLoader.Api.Unity;
using VenusRootLoader.TextAssetData.Items;

namespace VenusRootLoader.Api.Leaves;

public sealed class ItemLeaf : ILeaf<int>
{
    public sealed class ItemUse
    {
        public MainManager.ItemUsage Effect { get; set; }
        public int Value { get; set; }
    }

    public required int GameId { get; init; }
    public required string NamedId { get; init; }
    public required string CreatorId { get; init; }

    internal readonly WrappedSprite ItemSprite = new();
    internal readonly ItemData ItemData = new();
    internal Dictionary<int, ItemLanguageData> ItemLanguageData { get; } = new();

    public List<ItemUse> Effects => ItemData.Effects;
    public Dictionary<int, ItemLanguageData> LanguageData => ItemLanguageData;

    public Sprite Sprite
    {
        get => ItemSprite.Sprite;
        set => ItemSprite.Sprite = value;
    }

    public int BuyingPrice
    {
        get => ItemData.BuyingPrice;
        set => ItemData.BuyingPrice = value;
    }

    public BattleControl.AttackArea Target
    {
        get => ItemData.Target;
        set => ItemData.Target = value;
    }
}