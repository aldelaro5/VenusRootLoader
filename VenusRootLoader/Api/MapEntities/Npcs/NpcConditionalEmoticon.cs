using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Api.MapEntities.Npcs;

public sealed class NpcConditionalEmoticon
{
    public required Branch<FlagLeaf>? RequiredFlag { get; set; }
    public required int? EmoticonId { get; set; }
}