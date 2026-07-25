using VenusRootLoader.Api.Leaves.MapEntities.Behaviors.Enums;

namespace VenusRootLoader.Api.Leaves.MapEntities.Behaviors;

public sealed class ShootProjectileMapEntityBehavior : MapEntityBehavior
{
    internal ShootProjectileMapEntityBehavior(MapEntityLeaf mapEntityLeaf, BehaviorKind kind) : base(
        mapEntityLeaf,
        kind)
    {
        InternalTypeForKind = NPCControl.ActionBehaviors.ShootProjectile;
    }
}