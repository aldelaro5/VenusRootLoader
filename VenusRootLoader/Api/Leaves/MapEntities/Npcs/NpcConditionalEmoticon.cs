namespace VenusRootLoader.Api.Leaves.MapEntities.Npcs;

public sealed class NpcConditionalEmoticon
{
    public required Branch<FlagLeaf>? RequiredFlag { get; set; }
    public required NpcEmoticon Emoticon { get; set; }
}