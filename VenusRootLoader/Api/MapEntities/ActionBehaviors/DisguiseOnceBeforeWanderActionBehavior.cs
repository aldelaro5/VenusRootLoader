using VenusRootLoader.Api.MapEntities.ActionBehaviors.Enums;

namespace VenusRootLoader.Api.MapEntities.ActionBehaviors;

public sealed class DisguiseOnceBeforeWanderActionBehavior : ActionBehavior
{
    public float MaxFramesIntervalBeforeMovingAgain
    {
        get => InternalFrequencyForKind;
        set => InternalFrequencyForKind = value;
    }

    public float RadiusToWanderFromStartingPosition
    {
        get => MapEntity.InternalWanderRadius;
        set => MapEntity.InternalWanderRadius = value;
    }

    public float MaxDistanceFromStartingPositionBeforeTeleported
    {
        get => MapEntity.InternalTeleportRadius;
        set => MapEntity.InternalTeleportRadius = value;
    }

    internal DisguiseOnceBeforeWanderActionBehavior(MapEntity mapEntity, ActionBehaviorKind kind)
        : base(mapEntity, kind)
    {
        InternalTypeForKind = NPCControl.ActionBehaviors.DisguiseOnce;
    }
}