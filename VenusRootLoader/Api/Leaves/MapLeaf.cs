using System.Collections.ObjectModel;
using VenusRootLoader.Api.MapEntities;

namespace VenusRootLoader.Api.Leaves;

// TODO: This API needs several improvements before it's ready
// TODO: Figure out the MapControl config and Unity prefab tooling
public sealed class MapLeaf : Leaf
{
    internal List<MapEntity> InternalEntities { get; } = new();

    // TODO: Move this to the constructor so we can remove the null!
    public ReadOnlyCollection<MapEntity> Entities { get; internal set; } = null!;
    public LocalizedData<List<string>> Dialogues { get; } = new();

    public BeetleGrassMapEntity ReserveNewBeetleGrassEntity(string name)
    {
        BeetleGrassMapEntity newEntity = new()
        {
            Id = InternalEntities.Count,
            Name = name
        };
        InternalEntities.Add(newEntity);
        return newEntity;
    }
}