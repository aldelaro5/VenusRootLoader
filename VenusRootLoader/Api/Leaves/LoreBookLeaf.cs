namespace VenusRootLoader.Api.Leaves;

internal sealed class LoreBookLeaf : Leaf
{
    internal Dictionary<int, string> Title { get; } = new();
    internal Dictionary<int, string> Content { get; } = new();
}