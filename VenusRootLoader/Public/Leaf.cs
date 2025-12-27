namespace VenusRootLoader.Public;

public abstract class Leaf<T>
{
    public abstract T GameId { get; }
    public required string NamedId { get; init; }
    public required string CreatorId { get; init; }
    public required string OwnerId { get; init; }
}