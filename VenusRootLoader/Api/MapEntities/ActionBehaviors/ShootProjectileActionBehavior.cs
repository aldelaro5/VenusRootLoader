using VenusRootLoader.Api.MapEntities.ActionBehaviors.Enums;

namespace VenusRootLoader.Api.MapEntities.ActionBehaviors;

public sealed class ShootProjectileActionBehavior : ActionBehavior
{
    internal ShootProjectileActionBehavior(MapEntity mapEntity, ActionBehaviorKind kind) : base(mapEntity, kind)
    {
        InternalTypeForKind = NPCControl.ActionBehaviors.ShootProjectile;
    }
}