namespace VenusRootLoader.Api.Leaves;

public abstract class Leaf : ILeafIdentifier
{
    public int GameId { get; internal init; }
    public string NamedId { get; internal init; } = "";
    public string CreatorId { get; internal init; } = "";
}