using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves.RankBonuses;

[ExposeCreateFromVenus(typeof(RankBonusLeaf))]
public sealed class PartyMemberStatRankBonusLeaf : RankBonusLeaf
{
    internal PartyMemberStatRankBonusLeaf(int gameId, string creatorId, string namedId)
        : base(gameId, creatorId, namedId)
    {
    }

    internal override RankBonusType InternalBonusType => RankBonusType.GrantStatToPartyMember;

    public PartyMember Recipient
    {
        get => (PartyMember)InternalFirstParameter;
        set => InternalFirstParameter = (int)value;
    }

    public PartyMemberStatBonusType StatBonusType
    {
        get => (PartyMemberStatBonusType)InternalSecondParameter;
        set => InternalSecondParameter = (int)value;
    }

    public int AmountStatIncrease
    {
        get => InternalThirdParameter;
        set => InternalThirdParameter = value;
    }

    internal override void InitializeFromExisting() { }

    [LeafInitializeFromNew]
    internal void InitializeFromNew(
        int rankNeeded,
        PartyMember recipient,
        PartyMemberStatBonusType bonusType,
        int amountStatIncrease)
    {
        base.InitializeFromNew(rankNeeded);
        Recipient = recipient;
        StatBonusType = bonusType;
        AmountStatIncrease = amountStatIncrease;
    }
}