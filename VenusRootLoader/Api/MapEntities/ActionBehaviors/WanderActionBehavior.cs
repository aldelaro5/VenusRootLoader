using CommunityToolkit.Diagnostics;
using VenusRootLoader.Api.MapEntities.ActionBehaviors.Enums;

namespace VenusRootLoader.Api.MapEntities.ActionBehaviors;

public sealed class WanderActionBehavior : ActionBehavior
{
    public WanderBehaviorPattern WanderPattern
    {
        get => InternalTypeForKind switch
        {
            NPCControl.ActionBehaviors.Wander => WanderBehaviorPattern.Regular,
            NPCControl.ActionBehaviors.WanderUnderground => WanderBehaviorPattern.FromUnderground,
            NPCControl.ActionBehaviors.WanderOffscreen => WanderBehaviorPattern.CanWonderWhenInactive,
            NPCControl.ActionBehaviors.WanderNoWarp => WanderBehaviorPattern.WillNotWarpIfNoWanderPositionIsAvailable,
            NPCControl.ActionBehaviors.WanderOnWater => WanderBehaviorPattern.OnWater,
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<WanderBehaviorPattern>(nameof(InternalTypeForKind))
        };
        set => InternalTypeForKind = value switch
        {
            WanderBehaviorPattern.Regular => NPCControl.ActionBehaviors.Wander,
            WanderBehaviorPattern.FromUnderground => NPCControl.ActionBehaviors.WanderUnderground,
            WanderBehaviorPattern.CanWonderWhenInactive => NPCControl.ActionBehaviors.WanderOffscreen,
            WanderBehaviorPattern.WillNotWarpIfNoWanderPositionIsAvailable => NPCControl.ActionBehaviors.WanderNoWarp,
            WanderBehaviorPattern.OnWater => NPCControl.ActionBehaviors.WanderOnWater,
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<NPCControl.ActionBehaviors>(nameof(WanderPattern))
        };
    }

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

    internal WanderActionBehavior(MapEntity mapEntity, ActionBehaviorKind kind) : base(mapEntity, kind) { }
}