using VenusRootLoader.Api.Leaves.MapEntities.ActionBehaviors.Enums;

namespace VenusRootLoader.Api.Leaves.MapEntities.ActionBehaviors;

public sealed class DisguiseOnceBeforeWanderActionBehavior : ActionBehavior
{
    public float MaxFramesIntervalBeforeMovingAgain
    {
        get => InternalFrequencyForKind;
        set => InternalFrequencyForKind = value;
    }

    public float RadiusToWanderFromStartingPosition
    {
        get => MapEntityLeaf.InternalWanderRadius;
        set => MapEntityLeaf.InternalWanderRadius = value;
    }

    public float MaxDistanceFromStartingPositionBeforeTeleport
    {
        get => MapEntityLeaf.InternalTeleportRadius;
        set => MapEntityLeaf.InternalTeleportRadius = value;
    }

    internal DisguiseOnceBeforeWanderActionBehavior(MapEntityLeaf mapEntityLeaf, ActionBehaviorKind kind)
        : base(mapEntityLeaf, kind)
    {
        InternalTypeForKind = NPCControl.ActionBehaviors.DisguiseOnce;
    }
}