using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves.RankBonuses;

[ExposeCreateFromVenus(typeof(RankBonusLeaf))]
public sealed class WholePartyStatRankBonusLeaf : RankBonusLeaf
{
    internal WholePartyStatRankBonusLeaf(int gameId, string creatorId, string namedId)
        : base(gameId, creatorId, namedId)
    {
    }

    internal override RankBonusType InternalBonusType => RankBonusType.GrantStatToWholeParty;

    public WholePartyStatBonusType StatBonusType
    {
        get => (WholePartyStatBonusType)InternalFirstParameter;
        set => InternalFirstParameter = (int)value;
    }

    public int AmountStatIncrease
    {
        get => InternalSecondParameter;
        set => InternalSecondParameter = value;
    }

    internal override void InitializeFromExisting() { }

    [LeafInitializeFromNew]
    internal void InitializeFromNew(int rankNeeded, WholePartyStatBonusType bonusType, int amountStatIncrease)
    {
        base.InitializeFromNew(rankNeeded);
        StatBonusType = bonusType;
        AmountStatIncrease = amountStatIncrease;
    }
}