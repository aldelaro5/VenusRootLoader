namespace VenusRootLoader.Api.Leaves;

public abstract class Leaf : ILeafIdentifier
{
    public int GameId { get; }
    public string NamedId { get; }
    public string CreatorId { get; }

    private protected Leaf(int gameId, string namedId, string creatorId)
    {
        GameId = gameId;
        NamedId = namedId;
        CreatorId = creatorId;
    }
}