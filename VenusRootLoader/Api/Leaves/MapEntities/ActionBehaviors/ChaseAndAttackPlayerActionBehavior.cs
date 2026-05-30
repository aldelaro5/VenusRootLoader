using CommunityToolkit.Diagnostics;
using VenusRootLoader.Api.Leaves.MapEntities.ActionBehaviors.Enums;

namespace VenusRootLoader.Api.Leaves.MapEntities.ActionBehaviors;

public sealed class ChaseAndAttackPlayerActionBehavior : ActionBehavior
{
    public bool AttacksFromUnderground
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

    internal ChaseAndAttackPlayerActionBehavior(MapEntityLeaf mapEntityLeaf, ActionBehaviorKind kind) : base(
        mapEntityLeaf,
        kind)
    {
    }
}