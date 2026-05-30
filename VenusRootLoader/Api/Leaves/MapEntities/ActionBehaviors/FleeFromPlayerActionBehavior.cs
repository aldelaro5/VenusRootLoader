using VenusRootLoader.Api.Leaves.MapEntities.ActionBehaviors.Enums;

namespace VenusRootLoader.Api.Leaves.MapEntities.ActionBehaviors;

public sealed class FleeFromPlayerActionBehavior : ActionBehavior
{
    internal FleeFromPlayerActionBehavior(MapEntityLeaf mapEntityLeaf, ActionBehaviorKind kind) : base(
        mapEntityLeaf,
        kind)
    {
        InternalTypeForKind = NPCControl.ActionBehaviors.FleeFromPlayer;
    }
}