using VenusRootLoader.Api.MapEntities;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves;

// TODO: This API needs several improvements before it's ready
// TODO: Figure out the MapControl config and Unity prefab tooling
public sealed class MapLeaf : Leaf
{
    internal MapLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal ILeavesRegistry<MapEntity> EntitiesRegistry { get; set; } = null!;
    internal ILeavesRegistry<MapDialogueLeaf> DialoguesRegistry { get; set; } = null!;

    public IList<MapEntity> Entities => EntitiesRegistry.LeavesByGameIds.Values.ToList();
    public IList<MapDialogueLeaf> Dialogues => DialoguesRegistry.LeavesByGameIds.Values.ToList();

    public TMapEntity ReserveNewMapEntity<TMapEntity>(string namedId, string creatorId)
        where TMapEntity : MapEntity
    {
        TMapEntity newEntity = EntitiesRegistry.RegisterNew<TMapEntity>(namedId, creatorId);
        newEntity.BaseGameObjectName = namedId;
        newEntity.Map = this;
        newEntity.InitializeFromNew();
        return newEntity;
    }
}