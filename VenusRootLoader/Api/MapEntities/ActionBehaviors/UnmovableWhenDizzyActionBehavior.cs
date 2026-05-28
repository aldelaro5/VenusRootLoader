using VenusRootLoader.Api.MapEntities.ActionBehaviors.Enums;

namespace VenusRootLoader.Api.MapEntities.ActionBehaviors;

public sealed class UnmovableWhenDizzyActionBehavior : ActionBehavior
{
    internal UnmovableWhenDizzyActionBehavior(MapEntity mapEntity, ActionBehaviorKind kind) : base(
        mapEntity,
        kind)
    {
        InternalTypeForKind = NPCControl.ActionBehaviors.Unmoveable;
    }
}