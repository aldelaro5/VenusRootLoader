using VenusRootLoader.Api.MapEntities.ActionBehaviors.Enums;

namespace VenusRootLoader.Api.MapEntities.ActionBehaviors;

public sealed class DisguiseActionBehavior : ActionBehavior
{
    internal DisguiseActionBehavior(MapEntity mapEntity, ActionBehaviorKind kind) : base(mapEntity, kind)
    {
        InternalTypeForKind = NPCControl.ActionBehaviors.Disguise;
    }
}