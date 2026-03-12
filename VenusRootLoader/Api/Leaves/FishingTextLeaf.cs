namespace VenusRootLoader.Api.Leaves;

public sealed class FishingTextLeaf : Leaf
{
    public LocalizedData<string> LocalizedText { get; } = new();
}