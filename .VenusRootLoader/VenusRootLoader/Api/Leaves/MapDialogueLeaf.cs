namespace VenusRootLoader.Api.Leaves;

public sealed class MapDialogueLeaf : DialogueLeaf
{
    internal MapDialogueLeaf(int gameId, string creatorId, string namedId) : base(gameId, creatorId, namedId) { }
    internal override Branch<MapLeaf>? AssociatedMap => Map;
    public Branch<MapLeaf> Map { get; internal set; } = null!;
    public LocalizedData<string> LocalizedText { get; } = new();
}