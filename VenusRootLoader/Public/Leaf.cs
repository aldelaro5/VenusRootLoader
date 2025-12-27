namespace VenusRootLoader.Public;

public abstract class Leaf<T>
{
    public abstract T GameId { get; }
    public string NamedId { get; internal init; }
    public string CreatorId { get; internal init; }
    public string OwnerId { get; internal init; }

    internal Leaf(string namedId, string creatorId, string ownerId)
    {
        NamedId = namedId;
        CreatorId = creatorId;
        OwnerId = ownerId;
    }
}