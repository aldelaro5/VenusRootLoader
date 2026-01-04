namespace VenusRootLoader.Api.Leaves;

internal sealed class MedalLeaf : ILeaf
{
    internal sealed class MedalEffect
    {
        internal MainManager.BadgeEffects Effect { get; set; }
        internal int Value { get; set; }
    }

    public int GameId { get; init; }
    public string NamedId { get; init; } = "";
    public string CreatorId { get; init; } = "";

    internal Dictionary<int, string> Name { get; set; } = new();
    internal Dictionary<int, string> Description { get; set; } = new();
    internal Dictionary<int, string> Prepender { get; set; } = new();

    internal int MpCost { get; set; }
    internal bool IsPartyEquip { get; set; }
    internal List<MedalEffect> Effects { get; } = new();
    internal int BuyingPriceRegularBerries { get; set; }
    internal int BuyingPriceCrystalBerries { get; set; }
    internal int Items1SpriteIndex { get; set; } = -1;
}