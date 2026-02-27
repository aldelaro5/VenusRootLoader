namespace VenusRootLoader.Api.Leaves;

internal sealed class ActionCommandLeaf : Leaf
{
    internal Dictionary<int, string> Instructions { get; } = new();
}