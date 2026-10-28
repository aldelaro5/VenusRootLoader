using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves.RankBonuses;

[ExposeCreateFromVenus(typeof(RankBonusLeaf))]
public sealed class InventoryCapacityRankBonusLeaf : RankBonusLeaf
{
    internal InventoryCapacityRankBonusLeaf(int gameId, string creatorId, string namedId)
        : base(gameId, creatorId, namedId)
    {
    }

    internal override RankBonusType InternalBonusType => RankBonusType.GrantInventoryCapacity;

    public int AmountCapacityIncrease
    {
        get => InternalFirstParameter;
        set => InternalFirstParameter = value;
    }

    internal override void InitializeFromExisting() { }

    [LeafInitializeFromNew]
    internal void InitializeFromNew(int rankNeeded, int amountStatIncrease)
    {
        base.InitializeFromNew(rankNeeded);
        AmountCapacityIncrease = amountStatIncrease;
    }
}