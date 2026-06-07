using CommunityToolkit.Diagnostics;
using VenusRootLoader.Api.Leaves.MapEntities.Behaviors.Enums;

namespace VenusRootLoader.Api.Leaves.MapEntities.Behaviors;

public sealed class WanderMapEntityBehavior : MapEntityBehavior
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
        get => MapEntityLeaf.InternalWanderRadius;
        set => MapEntityLeaf.InternalWanderRadius = value;
    }

    public float MaxDistanceFromStartingPositionBeforeTeleport
    {
        get => MapEntityLeaf.InternalTeleportRadius;
        set => MapEntityLeaf.InternalTeleportRadius = value;
    }

    internal WanderMapEntityBehavior(MapEntityLeaf mapEntityLeaf, ActionBehaviorKind kind) : base(mapEntityLeaf, kind) { }
}