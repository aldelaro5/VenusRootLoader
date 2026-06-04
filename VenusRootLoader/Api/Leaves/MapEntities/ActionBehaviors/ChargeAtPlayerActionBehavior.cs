using CommunityToolkit.Diagnostics;
using VenusRootLoader.Api.Leaves.MapEntities.ActionBehaviors.Enums;

namespace VenusRootLoader.Api.Leaves.MapEntities.ActionBehaviors;

public sealed class ChargeAtPlayerActionBehavior : ActionBehavior
{
    public bool LockSpriteFlipDuringCharge
    {
        get => InternalTypeForKind switch
        {
            NPCControl.ActionBehaviors.ChargeAtPlayer => false,
            NPCControl.ActionBehaviors.ChargeAtPlayerFlipSprite => true,
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<bool>(nameof(InternalTypeForKind))
        };
        set => InternalTypeForKind = value
            ? NPCControl.ActionBehaviors.ChargeAtPlayerFlipSprite
            : NPCControl.ActionBehaviors.ChargeAtPlayer;
    }

    public float MovementSpeedMultiplier
    {
        get => MapEntityLeaf.InternalSpeedMultiplier;
        set => MapEntityLeaf.InternalSpeedMultiplier = value;
    }

    internal ChargeAtPlayerActionBehavior(MapEntityLeaf mapEntityLeaf, ActionBehaviorKind kind) : base(
        mapEntityLeaf,
        kind)
    {
    }
}