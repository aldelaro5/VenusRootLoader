namespace VenusRootLoader.Api.Leaves;

public abstract class DialogueLeaf : Leaf
{
    internal DialogueLeaf(int gameId, string namedId, string creatorId) : base(gameId, namedId, creatorId)
    {
    }

    internal abstract Branch<MapLeaf>? AssociatedMap { get; }
}