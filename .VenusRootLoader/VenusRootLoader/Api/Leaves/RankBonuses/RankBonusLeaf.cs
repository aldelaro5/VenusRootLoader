namespace VenusRootLoader.Api.Leaves.RankBonuses;

// We need to make this API easier instead of having 3 parameter values
public class RankBonusLeaf : Leaf
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

    public int RankNeeded { get; set; }
    internal RankBonusType InternalBonusType { get; set; }
    internal int InternalFirstParameter { get; set; }
    internal int InternalSecondParameter { get; set; }
    internal int InternalThirdParameter { get; set; }
}