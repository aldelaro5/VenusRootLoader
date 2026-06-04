using System.Collections.ObjectModel;
using UnityEngine;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.ActionBehaviors;

// TODO: Figure out if you can put a delay
// TODO: Should we split the type for the sleep start one?
public sealed class StealthSpotBehavior : ActionBehavior
{
    public float? DelayFramesBeforeMovingToNextNode
    {
        get => (int)MapEntityLeaf.InternalOutOfRangeActionFrequency != 5555
            ? MapEntityLeaf.InternalOutOfRangeActionFrequency
            : null;
        set => MapEntityLeaf.InternalOutOfRangeActionFrequency = value ?? 5555f;
    }

    public Branch<EventLeaf>? EventToStartWhenSpottingPlayer
    {
        get;
        set
        {
            MapEntityLeaf.InternalBattleEnemyIds[0] = value?.GameId ?? -1;
            field = value;
        }
    }

    public int VisionLengthInUnits
    {
        get => MapEntityLeaf.InternalBattleEnemyIds[1];
        set => MapEntityLeaf.InternalBattleEnemyIds[1] = value;
    }

    public ReadOnlyCollection<Vector3> MovementPathNodePositions { get; private set; } =
        new List<Vector3>().AsReadOnly();

    internal StealthSpotBehavior(MapEntityLeaf mapEntityLeaf) : base(mapEntityLeaf, null)
    {
        MapEntityLeaf.InternalOutOfRangeBehavior = NPCControl.ActionBehaviors.StealthAI;

        if (MapEntityLeaf.InternalBattleEnemyIds.Count < 2)
            MapEntityLeaf.InternalBattleEnemyIds.AddRange(
                Enumerable.Repeat(0, 2 - MapEntityLeaf.InternalBattleEnemyIds.Count));
    }

    internal void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        ILeavesRegistry<EventLeaf> eventsRegistry = registryResolver.Resolve<EventLeaf>();

        EventToStartWhenSpottingPlayer = MapEntityLeaf.InternalBattleEnemyIds[0] >= 0
            ? new(eventsRegistry.LeavesByGameIds[MapEntityLeaf.InternalBattleEnemyIds[0]])
            : null;
    }

    public void ChangeMovementPathNodePositions(ICollection<Vector3> nodes)
    {
        MapEntityLeaf.InternalSecondaryVectorData.Clear();
        MapEntityLeaf.InternalSecondaryVectorData.AddRange(nodes);
        MovementPathNodePositions = nodes.ToList().AsReadOnly();
    }
}