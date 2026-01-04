namespace VenusRootLoader.Api.Leaves;

internal sealed class LoreBookLeaf : ILeaf
{
    public int GameId { get; init; }
    public string NamedId { get; init; } = "";
    public string CreatorId { get; init; } = "";

    internal Dictionary<int, string> Title { get; } = new();
    internal Dictionary<int, string> Content { get; } = new();
}