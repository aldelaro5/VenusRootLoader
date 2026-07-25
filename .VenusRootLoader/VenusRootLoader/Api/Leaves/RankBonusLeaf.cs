namespace VenusRootLoader.Api.Leaves;

// TODO: We need to make this API easier instead of having 3 parameter values
internal sealed class RankBonusLeaf : Leaf
{
    internal enum RankBonusType
    {
        GrantSkill,
        GrantStatToPartyMember,
        GrantStatToWholeParty,
        GrantInventoryCapacity
    }

    internal RankBonusLeaf(int gameId, string namedId, string creatorId) : base(gameId, namedId, creatorId)
    {
    }

    internal int RankNeeded { get; set; }
    internal RankBonusType BonusType { get; set; }
    internal int FirstParameter { get; set; }
    internal int SecondParameter { get; set; }
    internal int ThirdParameter { get; set; }
}