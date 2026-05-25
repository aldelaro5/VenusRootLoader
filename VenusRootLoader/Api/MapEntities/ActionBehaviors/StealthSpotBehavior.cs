using System.Collections.ObjectModel;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.MapEntities.ActionBehaviors;

// TODO: Add path position nodes
// TODO: Figure out if you can put a delay
// TODO: Should we split the type for the sleep start one?
public sealed class StealthSpotBehavior : ActionBehavior
{
    public float? DelayFramesBeforeMovingToNextNode
    {
        get => (int)MapEntity.InternalPrimaryActionFrequency != 5555
            ? MapEntity.InternalPrimaryActionFrequency
            : null;
        set => MapEntity.InternalPrimaryActionFrequency = value ?? 5555f;
    }

    public Branch<EventLeaf>? EventToStartWhenSpottingPlayer
    {
        get;
        set
        {
            MapEntity.InternalBattleEnemyIds[0] = value?.GameId ?? -1;
            field = value;
        }
    }

    public int VisionLengthInUnits
    {
        get => MapEntity.InternalBattleEnemyIds[1];
        set => MapEntity.InternalBattleEnemyIds[1] = value;
    }

    public ReadOnlyCollection<Vector3> MovementPathNodePositions { get; private set; } =
        new List<Vector3>().AsReadOnly();

    internal StealthSpotBehavior(MapEntity mapEntity) : base(mapEntity, null)
    {
        MapEntity.InternalPrimaryBehavior = NPCControl.ActionBehaviors.StealthAI;

        if (MapEntity.InternalBattleEnemyIds.Count < 2)
            MapEntity.InternalBattleEnemyIds.AddRange(Enumerable.Repeat(0, 2 - MapEntity.InternalBattleEnemyIds.Count));
    }

    internal void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        ILeavesRegistry<EventLeaf> eventsRegistry = registryResolver.Resolve<EventLeaf>();

        EventToStartWhenSpottingPlayer = MapEntity.InternalBattleEnemyIds[0] >= 0
            ? new(eventsRegistry.LeavesByGameIds[MapEntity.InternalBattleEnemyIds[0]])
            : null;
    }

    public void ChangeMovementPathNodePositions(ICollection<Vector3> nodes)
    {
        MapEntity.InternalSecondaryVectorData.Clear();
        MapEntity.InternalSecondaryVectorData.AddRange(nodes);
        MovementPathNodePositions = nodes.ToList().AsReadOnly();
    }
}