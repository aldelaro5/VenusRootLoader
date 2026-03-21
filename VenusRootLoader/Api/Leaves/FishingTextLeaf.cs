namespace VenusRootLoader.Api.Leaves;

public sealed class FishingTextLeaf : Leaf
{
    internal FishingTextLeaf(int gameId, string namedId, string creatorId) : base(gameId, namedId, creatorId)
    {
    }

    public LocalizedData<string> LocalizedText { get; } = new();
}