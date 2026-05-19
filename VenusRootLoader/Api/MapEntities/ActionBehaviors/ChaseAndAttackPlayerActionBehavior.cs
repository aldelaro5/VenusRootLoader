using CommunityToolkit.Diagnostics;
using VenusRootLoader.Api.MapEntities.ActionBehaviors.Enums;

namespace VenusRootLoader.Api.MapEntities.ActionBehaviors;

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

    internal ChaseAndAttackPlayerActionBehavior(MapEntity mapEntity, ActionBehaviorKind kind) : base(mapEntity, kind)
    {
    }
}