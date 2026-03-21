namespace VenusRootLoader.Api.Leaves;

public sealed class PrizeMedalLeaf : Leaf
{
    internal PrizeMedalLeaf(int gameId, string namedId, string creatorId) : base(gameId, namedId, creatorId)
    {
    }

    public int MedalGameId { get; set; }
    public int FlagvarGameId { get; set; }
    public int DisplayedEnemyGameId { get; set; }
}