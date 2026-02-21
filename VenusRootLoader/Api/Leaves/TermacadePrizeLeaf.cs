namespace VenusRootLoader.Api.Leaves;

public sealed class TermacadePrizeLeaf : ILeaf
{
    public enum TermacadePrizeType
    {
        StandardItem,
        KeyItem,
        Medal
    }
    
    public int GameId { get; init; }
    public string NamedId { get; init; } = "";
    public string CreatorId { get; init; } = "";

    public TermacadePrizeType PrizeType { get; set; }
    public int ItemOrMedalGameId { get; set; }
    public int GameTokenCost { get; set; }
    public int? AlreadyBoughtFlagGameId { get; set; }
}