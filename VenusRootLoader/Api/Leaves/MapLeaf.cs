using System.Collections.ObjectModel;
using System.Reflection;
using VenusRootLoader.Api.MapEntities;

namespace VenusRootLoader.Api.Leaves;

// TODO: This API needs several improvements before it's ready
// TODO: Figure out the MapControl config and Unity prefab tooling
public sealed class MapLeaf : Leaf
{
    internal MapLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
        Entities = new ReadOnlyCollection<MapEntity>(InternalEntities);
    }

    internal List<MapEntity> InternalEntities { get; } = new();

    public ReadOnlyCollection<MapEntity> Entities { get; }

    public LocalizedData<List<string>> Dialogues { get; } = new();

    public T ReserveNewMapEntity<T>(string name)
        where T : MapEntity
    {
        T newEntity = (T)Activator.CreateInstance(
            typeof(T),
            BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.NonPublic,
            null,
            null,
            null,
            null);

        newEntity.Id = InternalEntities.Count;
        newEntity.Name = name;
        newEntity.InitializeFromNew();
        InternalEntities.Add(newEntity);
        return newEntity;
    }
}