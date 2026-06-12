using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Api.Leaves.MapEntities;
using VenusRootLoader.Api.Leaves.MapEntities.Enemies;

namespace VenusRootLoader.Api;

public partial class Venus
{
    public EnemyEncounterDroppingItemsMapEntityLeaf RegisterEncounterDroppingItemsMapEntityLeaf(
        string namedId,
        MapLeaf map,
        Vector3 startingPosition,
        Branch<AnimIdLeaf> animId,
        IList<Branch<EnemyLeaf>> enemies,
        IList<EnemyItemDrop> itemsDropped)
    {
        var mapEntity = RegisterMapEntity<EnemyEncounterDroppingItemsMapEntityLeaf>(namedId, map);
        mapEntity.InitializeFromNew(startingPosition, animId, enemies, itemsDropped);
        return mapEntity;
    }

    private TMapEntity RegisterMapEntity<TMapEntity>(string namedId, MapLeaf map)
        where TMapEntity : MapEntityLeaf
    {
        TMapEntity mapEntityLeaf = map.EntitiesRegistry.RegisterNew<TMapEntity>(namedId, BudId);
        mapEntityLeaf.BaseGameObjectName = namedId;
        mapEntityLeaf.Map = map;
        return mapEntityLeaf;
    }
}