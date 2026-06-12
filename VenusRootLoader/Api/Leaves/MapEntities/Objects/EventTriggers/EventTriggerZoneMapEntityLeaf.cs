using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.EventTriggers;

public sealed class EventTriggerZoneMapEntityLeaf : EventTriggerMapEntityLeaf
{
    internal EventTriggerZoneMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    public Branch<EventLeaf> EventToStartWhenTriggered
    {
        get;
        set
        {
            InternalData[0].Value = value.GameId;
            field = value;
        }
    }

    public bool IsOneShotTrigger { get => InternalData[1].Value != 1; set => InternalData[1].Value = value ? 0 : 1; }

    public Vector3 TriggerBoxColliderSize { get => InternalBoxColSize; set => InternalBoxColSize = value; }
    public Vector3 TriggerBoxColliderCenter { get => InternalBoxColCenter; set => InternalBoxColCenter = value; }

    public Branch<FlagLeaf>? ActivationFlag
    {
        get;
        set
        {
            InternalActivationFlagId = value?.GameId ?? -1;
            field = value;
        }
    }

    internal void InitializeFromNew(
        Vector3 startingPosition,
        Branch<EventLeaf> eventToStartWhenTriggered,
        Vector3 triggerBoxColliderSize,
        Vector3 triggerBoxColliderCenter)
    {
        InternalData.AddRange([new(-1), new(0), new(0)]);
        EventToStartWhenTriggered = eventToStartWhenTriggered;
        InternalHaxBoxCol = true;
        InternalBoxColIsTrigger = true;
        TriggerBoxColliderSize = triggerBoxColliderSize;
        TriggerBoxColliderCenter = triggerBoxColliderCenter;
        EntityStartingPosition = startingPosition;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalData.Count < 3)
            InternalData.AddRange(Enumerable.Repeat(new Ref<int>(0), 3 - InternalData.Count));

        ILeavesRegistry<EventLeaf> eventsRegistry = registryResolver.Resolve<EventLeaf>();
        ILeavesRegistry<FlagLeaf> flagsRegistry = registryResolver.Resolve<FlagLeaf>();
        EventToStartWhenTriggered = new(eventsRegistry.LeavesByGameIds[InternalData[0].Value]);

        if (InternalActivationFlagId > 0)
            ActivationFlag = new(flagsRegistry.LeavesByGameIds[InternalActivationFlagId]);
    }
}