using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.ActionBehaviors;

public sealed class StealthSpotWhileAsleepBehavior : ActionBehavior
{
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

    internal StealthSpotWhileAsleepBehavior(MapEntityLeaf mapEntityLeaf) : base(mapEntityLeaf, null)
    {
        MapEntityLeaf.InternalOutOfRangeBehavior = NPCControl.ActionBehaviors.StealthAI;
        MapEntityLeaf.InternalOutOfRangeActionFrequency = 5555;

        if (MapEntityLeaf.InternalBattleEnemyIds.Count < 2)
        {
            MapEntityLeaf.InternalBattleEnemyIds.AddRange(
                Enumerable.Repeat(new Ref<int>(0), 2 - MapEntityLeaf.InternalBattleEnemyIds.Count));
        }
    }

    internal void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        ILeavesRegistry<EventLeaf> eventsRegistry = registryResolver.Resolve<EventLeaf>();

        EventToStartWhenSpottingPlayer = MapEntityLeaf.InternalBattleEnemyIds[0].Value >= 0
            ? new(eventsRegistry.LeavesByGameIds[MapEntityLeaf.InternalBattleEnemyIds[0].Value])
            : null;
    }
}