using VenusRootLoader.Api.Leaves.MapEntities.Behaviors.Enums;

namespace VenusRootLoader.Api.Leaves.MapEntities.Behaviors;

public sealed class WanderWhenAnimstateIsWalkOrIdleMapEntityBehavior : MapEntityBehavior
{
    public int AnimstateOverrideWhenNotWalkOrIdle
    {
        get => (int)InternalFrequencyForKind;
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

    internal WanderWhenAnimstateIsWalkOrIdleMapEntityBehavior(MapEntityLeaf mapEntityLeaf, BehaviorKind kind)
        : base(mapEntityLeaf, kind)
    {
        InternalTypeForKind = NPCControl.ActionBehaviors.WalkWhenAnim;
    }
}