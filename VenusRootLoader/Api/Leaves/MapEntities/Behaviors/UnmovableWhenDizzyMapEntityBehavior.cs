using VenusRootLoader.Api.Leaves.MapEntities.Behaviors.Enums;

namespace VenusRootLoader.Api.Leaves.MapEntities.Behaviors;

public sealed class UnmovableWhenDizzyMapEntityBehavior : MapEntityBehavior
{
    internal UnmovableWhenDizzyMapEntityBehavior(MapEntityLeaf mapEntityLeaf, BehaviorKind kind) : base(
        mapEntityLeaf,
        kind)
    {
        InternalTypeForKind = NPCControl.ActionBehaviors.Unmoveable;
    }
}