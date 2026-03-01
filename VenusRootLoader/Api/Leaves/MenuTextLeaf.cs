namespace VenusRootLoader.Api.Leaves;

public sealed class MenuTextLeaf : Leaf
{
    public LocalizedData<string> LocalizedText { get; } = new();
}