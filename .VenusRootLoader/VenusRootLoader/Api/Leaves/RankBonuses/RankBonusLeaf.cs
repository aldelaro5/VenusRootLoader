namespace VenusRootLoader.Api.Leaves.RankBonuses;

public abstract class RankBonusLeaf : Leaf
{
    internal enum RankBonusType
    {
        GrantSkill,
        GrantStatToPartyMember,
        GrantStatToWholeParty,
        GrantInventoryCapacity
    }

    internal RankBonusLeaf(int gameId, string creatorId, string namedId) : base(gameId, creatorId, namedId)
    {
    }

    internal abstract void InitializeFromExisting();

    internal virtual void InitializeFromNew(int rankNeeded)
    {
        RankNeeded = rankNeeded;
    }

    internal abstract RankBonusType InternalBonusType { get; }

    public int RankNeeded { get; set; }
    internal int InternalFirstParameter { get; set; }
    internal int InternalSecondParameter { get; set; }
    internal int InternalThirdParameter { get; set; }
}