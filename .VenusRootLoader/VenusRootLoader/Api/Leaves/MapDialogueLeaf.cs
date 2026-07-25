namespace VenusRootLoader.Api.Leaves;

public sealed class MapDialogueLeaf : DialogueLeaf
{
    internal MapDialogueLeaf(int gameId, string namedId, string creatorId) : base(gameId, namedId, creatorId) { }
    internal override Branch<MapLeaf>? AssociatedMap => Map;
    public Branch<MapLeaf> Map { get; internal set; }
    public LocalizedData<string> LocalizedText { get; } = new();
}