namespace VenusRootLoader.Api.Leaves.MapEntities.Npcs;

public sealed class NpcConditionalDialogue
{
    public required Branch<FlagLeaf>? Flag { get; set; }
    public required Branch<DialogueLeaf> Dialogue { get; set; }
    public required int DefaultAnimStateWhenSelected { get; set; }
}