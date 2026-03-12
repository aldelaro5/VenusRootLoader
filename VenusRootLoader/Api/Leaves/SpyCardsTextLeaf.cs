namespace VenusRootLoader.Api.Leaves;

public sealed class SpyCardsTextLeaf : Leaf
{
    public LocalizedData<string> LocalizedText { get; } = new();
}