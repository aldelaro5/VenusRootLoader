using CommunityToolkit.Diagnostics;
using VenusRootLoader.Api.Leaves.MapEntities.Behaviors.Enums;

namespace VenusRootLoader.Api.Leaves.MapEntities.Behaviors;

public sealed class ChaseAndAttackPlayerMapEntityBehavior : MapEntityBehavior
{
    public bool AttackFromUnderground
    {
        get => InternalTypeForKind switch
        {
            NPCControl.ActionBehaviors.ChargeAndAttack => false,
            NPCControl.ActionBehaviors.ChargeAttackUnderground => true,
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<bool>(nameof(InternalTypeForKind))
        };
        set => InternalTypeForKind = value
            ? NPCControl.ActionBehaviors.ChargeAttackUnderground
            : NPCControl.ActionBehaviors.ChargeAndAttack;
    }

    public float MinimumDistanceFromPlayerBeforeAttacking
    {
        get => InternalFrequencyForKind;
        set => InternalFrequencyForKind = value;
    }

    public float MovementSpeedMultiplier
    {
        get => MapEntityLeaf.InternalSpeedMultiplier;
        set => MapEntityLeaf.InternalSpeedMultiplier = value;
    }

    internal ChaseAndAttackPlayerMapEntityBehavior(MapEntityLeaf mapEntityLeaf, BehaviorKind kind) : base(
        mapEntityLeaf,
        kind)
    {
    }
}