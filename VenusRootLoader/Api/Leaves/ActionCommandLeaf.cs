namespace VenusRootLoader.Api.Leaves;

internal sealed class ActionCommandLeaf : ILeaf
{
    public int GameId { get; init; }
    public string NamedId { get; init; } = "";
    public string CreatorId { get; init; } = "";

    internal Dictionary<int, string> Instructions { get; } = new();
}