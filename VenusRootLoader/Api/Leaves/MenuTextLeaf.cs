namespace VenusRootLoader.Api.Leaves;

public sealed class MenuTextLeaf : Leaf
{
    internal MenuTextLeaf(int gameId, string namedId, string creatorId) : base(gameId, namedId, creatorId)
    {
    }

    public LocalizedData<string> LocalizedText { get; } = new();
}