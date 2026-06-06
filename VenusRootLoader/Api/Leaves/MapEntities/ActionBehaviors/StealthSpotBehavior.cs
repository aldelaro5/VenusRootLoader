using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.ActionBehaviors;

// TODO: Figure out if you can put a delay
public sealed class StealthSpotBehavior : ActionBehavior
{
    public float DelayFramesBeforeMovingToNextNode
    {
        get => MapEntityLeaf.InternalOutOfRangeActionFrequency;
        set => MapEntityLeaf.InternalOutOfRangeActionFrequency = value;
    }

    public Branch<EventLeaf>? EventToStartWhenSpottingPlayer
    {
        get;
        set
        {
            MapEntityLeaf.InternalBattleEnemyIds[0].Value = value?.GameId ?? -1;
            field = value;
        }
    }

    public int VisionLengthInUnits
    {
        get => MapEntityLeaf.InternalBattleEnemyIds[1].Value;
        set => MapEntityLeaf.InternalBattleEnemyIds[1].Value = value;
    }

    private readonly ListRefWrapper<Vector3, Vector3> _movementPathNodePositions;
    public IList<Vector3> MovementPathNodePositions => _movementPathNodePositions;

    internal StealthSpotBehavior(MapEntityLeaf mapEntityLeaf) : base(mapEntityLeaf, null)
    {
        _movementPathNodePositions = new(MapEntityLeaf.InternalSecondaryVectorData, 0, x => new(x));
        MapEntityLeaf.InternalOutOfRangeBehavior = NPCControl.ActionBehaviors.StealthAI;

        if (MapEntityLeaf.InternalBattleEnemyIds.Count < 2)
        {
            MapEntityLeaf.InternalBattleEnemyIds.AddRange(
                Enumerable.Repeat(new Ref<int>(0), 2 - MapEntityLeaf.InternalBattleEnemyIds.Count));
        }

        _movementPathNodePositions.SynchronizeFromExistingData(
            MapEntityLeaf.InternalSecondaryVectorData.Select(x => x.Value).ToList());
    }

    internal void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        ILeavesRegistry<EventLeaf> eventsRegistry = registryResolver.Resolve<EventLeaf>();

        EventToStartWhenSpottingPlayer = MapEntityLeaf.InternalBattleEnemyIds[0].Value >= 0
            ? new(eventsRegistry.LeavesByGameIds[MapEntityLeaf.InternalBattleEnemyIds[0].Value])
            : null;
    }
}