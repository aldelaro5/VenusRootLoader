using CommunityToolkit.Diagnostics;
using VenusRootLoader.Api.MapEntities.ActionBehaviors.Enums;

namespace VenusRootLoader.Api.MapEntities.ActionBehaviors;

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

    internal ChargeAtPlayerActionBehavior(MapEntity mapEntity, ActionBehaviorKind kind) : base(
        mapEntity,
        kind)
    {
    }
}