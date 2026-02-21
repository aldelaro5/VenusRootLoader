using UnityEngine;
using VenusRootLoader.Unity;

namespace VenusRootLoader.Api.Leaves;

public sealed class ItemLeaf : ILeaf
{
    public sealed class ItemUse
    {
        public MainManager.ItemUsage Effect { get; set; }
        public int Value { get; set; }
    }

    public sealed class ItemLanguageData
    {
        public string Name { get; set; } = "<NO NAME>";
        public string UnusedDescription { get; set; } = "";
        public string Description { get; set; } = "<NO DESCRIPTION>";
        public string? Prepender { get; set; }
    }

    internal WrappedSprite WrappedSprite = new();

    public int GameId { get; init; }
    public string NamedId { get; init; } = "";
    public string CreatorId { get; init; } = "";

    public List<ItemUse> Effects { get; } = new();
    public Dictionary<int, ItemLanguageData> LanguageData { get; } = new();

    public Sprite Sprite
    {
        get => WrappedSprite.Sprite;
        set => WrappedSprite.Sprite = value;
    }

    public int BuyingPrice { get; set; }
    public BattleControl.AttackArea Target { get; set; }
}