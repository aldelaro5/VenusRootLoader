using UnityEngine;
using VenusRootLoader.Api.Unity;
using VenusRootLoader.Unity;

namespace VenusRootLoader.Api.Leaves;

public sealed class ItemLeaf : ILeaf<int>
{
    public sealed class ItemUse
    {
        public MainManager.ItemUsage Effect { get; set; }
        public int Value { get; set; }
    }

    public sealed class ItemLanguageData
    {
        internal string Name { get; set; } = "<NO NAME>";
        internal string UnusedDescription { get; set; } = "";
        internal string Description { get; set; } = "<NO DESCRIPTION>";
        internal string? Prepender { get; set; }
    }

    internal WrappedSprite WrappedSprite = new();

    public required int GameId { get; init; }
    public required string NamedId { get; init; }
    public required string CreatorId { get; init; }

    public List<ItemUse> Effects { get; } = new();
    public Dictionary<int, ItemLanguageData> LanguageData { get; } = new();
    public Sprite Sprite => WrappedSprite.Sprite;
    public int BuyingPrice { get; set; }
    public BattleControl.AttackArea Target { get; set; }
}