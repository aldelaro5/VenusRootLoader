using VenusRootLoader.Api.MapEntities.ActionBehaviors.Enums;

namespace VenusRootLoader.Api.MapEntities.ActionBehaviors;

public sealed class FleeFromPlayerActionBehavior : ActionBehavior
{
    internal FleeFromPlayerActionBehavior(MapEntity mapEntity, ActionBehaviorKind kind) : base(
        mapEntity,
        kind)
    {
        InternalTypeForKind = NPCControl.ActionBehaviors.FleeFromPlayer;
    }
}