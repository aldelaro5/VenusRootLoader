using VenusRootLoader.Api.Leaves.MapEntities.ActionBehaviors.Enums;

namespace VenusRootLoader.Api.Leaves.MapEntities.ActionBehaviors;

public sealed class WanderWhenAnimstateIsWalkOrIdleActionBehavior : ActionBehavior
{
    public int AnimstateOverrideWhenNotChase
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

    internal WanderWhenAnimstateIsWalkOrIdleActionBehavior(MapEntityLeaf mapEntityLeaf, ActionBehaviorKind kind)
        : base(mapEntityLeaf, kind)
    {
        InternalTypeForKind = NPCControl.ActionBehaviors.WalkWhenAnim;
    }
}