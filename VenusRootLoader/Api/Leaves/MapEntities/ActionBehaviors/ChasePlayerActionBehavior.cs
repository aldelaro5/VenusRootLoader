using CommunityToolkit.Diagnostics;
using VenusRootLoader.Api.Leaves.MapEntities.ActionBehaviors.Enums;

namespace VenusRootLoader.Api.Leaves.MapEntities.ActionBehaviors;

public sealed class ChasePlayerActionBehavior : ActionBehavior
{
    public bool ChaseOnWater
    {
        get => InternalTypeForKind switch
        {
            NPCControl.ActionBehaviors.ChasePlayer => false,
            NPCControl.ActionBehaviors.ChaseOnWater => true,
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<bool>(nameof(InternalTypeForKind))
        };
        set => InternalTypeForKind = value
            ? NPCControl.ActionBehaviors.ChaseOnWater
            : NPCControl.ActionBehaviors.ChasePlayer;
    }

    public float MovementSpeedMultiplier
    {
        get => MapEntityLeaf.InternalSpeedMultiplier;
        set => MapEntityLeaf.InternalSpeedMultiplier = value;
    }

    internal ChasePlayerActionBehavior(MapEntityLeaf mapEntityLeaf, ActionBehaviorKind kind) : base(mapEntityLeaf, kind)
    {
    }
}