namespace VenusRootLoader.Api.Leaves;

public sealed class FlagvarLeaf : ILeaf
{
    public int GameId { get; init; }
    public string NamedId { get; init; } = "";
    public string CreatorId { get; init; } = "";
}