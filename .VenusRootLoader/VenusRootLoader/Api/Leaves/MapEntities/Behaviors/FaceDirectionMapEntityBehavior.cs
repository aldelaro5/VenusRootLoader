using CommunityToolkit.Diagnostics;
using VenusRootLoader.Api.Leaves.MapEntities.Behaviors.Enums;

namespace VenusRootLoader.Api.Leaves.MapEntities.Behaviors;

public sealed class FaceDirectionMapEntityBehavior : MapEntityBehavior
{
    public FacingBehaviorDirection FacingDirection
    {
        get => InternalTypeForKind switch
        {
            NPCControl.ActionBehaviors.FacePlayer => FacingBehaviorDirection.TowardsPlayer,
            NPCControl.ActionBehaviors.FaceAwayFromPlayer => FacingBehaviorDirection.AwayFromPlayer,
            NPCControl.ActionBehaviors.FaceAhead => FacingBehaviorDirection.TowardsEntityRightVector,
            NPCControl.ActionBehaviors.FaceBehind => FacingBehaviorDirection.TowardsEntityLeftVector,
            NPCControl.ActionBehaviors.FaceUp => FacingBehaviorDirection.TowardsEntityForwardVector,
            NPCControl.ActionBehaviors.FaceDown => FacingBehaviorDirection.TowardsEntityBackwardVector,
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<FacingBehaviorDirection>(nameof(InternalTypeForKind))
        };
        set => InternalTypeForKind = value switch
        {
            FacingBehaviorDirection.TowardsPlayer => NPCControl.ActionBehaviors.FacePlayer,
            FacingBehaviorDirection.AwayFromPlayer => NPCControl.ActionBehaviors.FaceAwayFromPlayer,
            FacingBehaviorDirection.TowardsEntityRightVector => NPCControl.ActionBehaviors.FaceAhead,
            FacingBehaviorDirection.TowardsEntityLeftVector => NPCControl.ActionBehaviors.FaceBehind,
            FacingBehaviorDirection.TowardsEntityForwardVector => NPCControl.ActionBehaviors.FaceUp,
            FacingBehaviorDirection.TowardsEntityBackwardVector => NPCControl.ActionBehaviors.FaceDown,
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<NPCControl.ActionBehaviors>(nameof(FacingDirection))
        };
    }

    internal FaceDirectionMapEntityBehavior(MapEntityLeaf mapEntityLeaf, BehaviorKind kind) : base(
        mapEntityLeaf,
        kind)
    {
    }
}