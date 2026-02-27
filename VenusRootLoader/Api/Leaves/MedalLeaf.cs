using UnityEngine;
using VenusRootLoader.Unity;

namespace VenusRootLoader.Api.Leaves;

public sealed class MedalLeaf : Leaf
{
    public sealed class MedalEffect
    {
        public MainManager.BadgeEffects Effect { get; set; }
        public int Value { get; set; }
    }

    public sealed class MedalLanguageData
    {
        public string Name { get; set; } = "<NO NAME>";
        public string Description { get; set; } = "<NO DESCRIPTION>";
        public string Prepender { get; set; } = "<NO PREPENDER>";
    }

    internal WrappedSprite WrappedSprite = new();

    internal int Items1SpriteIndex { get; set; } = -1;

    public int MpCost { get; set; }
    public bool IsPartyEquip { get; set; }
    public List<MedalEffect> Effects { get; } = new();
    public int BuyingPriceRegularBerries { get; set; }
    public int BuyingPriceCrystalBerries { get; set; }
    public Dictionary<int, MedalLanguageData> LanguageData { get; } = new();

    public Sprite Sprite
    {
        get => WrappedSprite.Sprite;
        set => WrappedSprite.Sprite = value;
    }
}