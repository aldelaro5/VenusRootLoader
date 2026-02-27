namespace VenusRootLoader.Api.Leaves;

internal sealed class MusicLeaf : Leaf
{
    internal Dictionary<int, string> Title { get; set; } = new();

    internal float EndBoundaryInSeconds { get; set; }
    internal float RestartBoundaryInSeconds { get; set; }
}