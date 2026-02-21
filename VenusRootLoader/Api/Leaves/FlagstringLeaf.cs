namespace VenusRootLoader.Api.Leaves;

public sealed class FlagstringLeaf : ILeaf
{
    public int GameId { get; init; }
    public string NamedId { get; init; } = "";
    public string CreatorId { get; init; } = "";
}