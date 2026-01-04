namespace VenusRootLoader.Api.Leaves;

internal sealed class TermacadePrizeLeaf : ILeaf
{
    public int GameId { get; init; }
    public string NamedId { get; init; } = "";
    public string CreatorId { get; init; } = "";

    internal int Type { get; set; }
    internal int ItemOrMedalId { get; set; }
    internal int GameTokenCost { get; set; }
    internal int Availability { get; set; }
    internal int BoundPurchasedFlagId { get; set; }
}