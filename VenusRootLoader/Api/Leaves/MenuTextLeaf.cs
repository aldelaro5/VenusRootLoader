namespace VenusRootLoader.Api.Leaves;

public sealed class MenuTextLeaf : Leaf
{
    public Dictionary<int, string> Text { get; } = new();
}