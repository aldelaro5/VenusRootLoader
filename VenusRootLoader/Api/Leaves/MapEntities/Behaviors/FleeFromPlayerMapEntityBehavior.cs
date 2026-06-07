using VenusRootLoader.Api.Leaves.MapEntities.Behaviors.Enums;

namespace VenusRootLoader.Api.Leaves.MapEntities.Behaviors;

public sealed class FleeFromPlayerMapEntityBehavior : MapEntityBehavior
{
    internal FleeFromPlayerMapEntityBehavior(MapEntityLeaf mapEntityLeaf, ActionBehaviorKind kind) : base(
        mapEntityLeaf,
        kind)
    {
        InternalTypeForKind = NPCControl.ActionBehaviors.FleeFromPlayer;
    }
}