namespace VenusRootLoader.Api.Leaves;

// TODO: We need to make this API easier instead of having 3 parameter values, add the Venus APIs when this is done
public sealed class RankBonusLeaf : Leaf
{
    public enum RankBonusType
    {
        GrantSkill,
        GrantStatToPartyMember,
        GrantStatToWholeParty,
        GrantInventoryCapacity
    }

    public int RankNeeded { get; set; }
    public RankBonusType BonusType { get; set; }
    public int FirstParameter { get; set; }
    public int SecondParameter { get; set; }
    public int ThirdParameter { get; set; }
}