namespace VenusRootLoader.Api.Leaves;

public closed class DialogueLeaf : Leaf
{
    internal DialogueLeaf(int gameId, string creatorId, string namedId) : base(gameId, creatorId, namedId)
    {
    }

    internal abstract Branch<MapLeaf>? AssociatedMap { get; }
}