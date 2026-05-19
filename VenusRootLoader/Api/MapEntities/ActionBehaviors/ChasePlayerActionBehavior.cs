using CommunityToolkit.Diagnostics;
using VenusRootLoader.Api.MapEntities.ActionBehaviors.Enums;

namespace VenusRootLoader.Api.MapEntities.ActionBehaviors;

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

    internal ChasePlayerActionBehavior(MapEntity mapEntity, ActionBehaviorKind kind) : base(mapEntity, kind) { }
}