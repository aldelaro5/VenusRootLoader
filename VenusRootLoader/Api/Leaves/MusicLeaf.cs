namespace VenusRootLoader.Api.Leaves;

internal sealed class MusicLeaf : ILeaf
{
    public int GameId { get; init; }
    public string NamedId { get; init; } = "";
    public string CreatorId { get; init; } = "";

    internal Dictionary<int, string> Title { get; set; } = new();

    internal float EndBoundaryInSeconds { get; set; }
    internal float RestartBoundaryInSeconds { get; set; }
}