namespace VenusRootLoader.Api.Leaves;

public sealed class TermacadePrizeLeaf : Leaf
{
    public enum TermacadePrizeType
    {
        StandardItem,
        KeyItem,
        Medal
    }

    public TermacadePrizeType PrizeType { get; set; }
    public int ItemOrMedalGameId { get; set; }
    public int GameTokenCost { get; set; }
    public int? AlreadyBoughtFlagGameId { get; set; }
}