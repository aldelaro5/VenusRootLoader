using VenusRootLoader.Api.Leaves.MapEntities.Behaviors.Enums;

namespace VenusRootLoader.Api.Leaves.MapEntities.Behaviors;

public sealed class DisguiseOnceBeforeWanderMapEntityBehavior : MapEntityBehavior
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

    internal DisguiseOnceBeforeWanderMapEntityBehavior(MapEntityLeaf mapEntityLeaf, BehaviorKind kind)
        : base(mapEntityLeaf, kind)
    {
        InternalTypeForKind = NPCControl.ActionBehaviors.DisguiseOnce;
    }
}