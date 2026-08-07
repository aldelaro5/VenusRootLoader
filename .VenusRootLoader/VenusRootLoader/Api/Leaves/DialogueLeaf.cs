namespace VenusRootLoader.Api.Leaves;

public abstract class DialogueLeaf : Leaf
{
    internal DialogueLeaf(int gameId, string creatorId, string namedId) : base(gameId, creatorId, namedId)
    {
    }

    internal abstract Branch<MapLeaf>? AssociatedMap { get; }
}