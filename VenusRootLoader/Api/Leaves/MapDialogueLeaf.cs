namespace VenusRootLoader.Api.Leaves;

public sealed class MapDialogueLeaf : Leaf
{
    internal MapDialogueLeaf(int gameId, string namedId, string creatorId) : base(gameId, namedId, creatorId) { }
    public Branch<MapLeaf> Map { get; internal set; }
    public LocalizedData<string> LocalizedText { get; } = new();
}