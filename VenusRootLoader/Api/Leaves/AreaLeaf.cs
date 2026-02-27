namespace VenusRootLoader.Api.Leaves;

internal sealed class AreaLeaf : Leaf
{
    internal Dictionary<int, string> Name { get; } = new();
    internal Dictionary<int, List<string>> PaginatedDescription { get; } = new();
}