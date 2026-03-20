using VenusRootLoader.Api.MapEntities;

namespace VenusRootLoader.Api.Leaves;

// TODO: This API needs several improvements before it's ready
// TODO: Figure out the MapControl config and Unity prefab tooling
public sealed class MapLeaf : Leaf
{
    public ReadOnlyListWithCreate<MapEntity> Entities { get; } = new();
    public LocalizedData<List<string>> Dialogues { get; } = new();
}