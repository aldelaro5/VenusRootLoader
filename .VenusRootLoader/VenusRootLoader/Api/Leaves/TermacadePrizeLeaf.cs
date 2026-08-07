using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves;

[ExposeFromVenus]
public sealed class TermacadePrizeLeaf : Leaf
{
    public enum TermacadePrizeType
    {
        StandardItem,
        KeyItem,
        Medal
    }

    internal TermacadePrizeLeaf(int gameId, string creatorId, string namedId) : base(gameId, creatorId, namedId)
    {
    }

    public TermacadePrizeType PrizeType { get; set; }
    public int ItemOrMedalGameId { get; set; }
    public int GameTokenCost { get; set; }
    public int? AlreadyBoughtFlagGameId { get; set; }
}