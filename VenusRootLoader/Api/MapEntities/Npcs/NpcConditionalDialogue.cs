using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Api.MapEntities.Npcs;

public sealed class NpcConditionalDialogue
{
    public required Branch<FlagLeaf>? Flag { get; set; }
    public required int DialogueId { get; set; }
    public required int DefaultAnimStateWhenSelected { get; set; }
}