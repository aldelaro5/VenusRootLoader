using VenusRootLoader.Api.Leaves.MapEntities.Behaviors.Enums;

namespace VenusRootLoader.Api.Leaves.MapEntities.Behaviors;

public sealed class DisguiseMapEntityBehavior : MapEntityBehavior
{
    internal DisguiseMapEntityBehavior(MapEntityLeaf mapEntityLeaf, ActionBehaviorKind kind) : base(mapEntityLeaf, kind)
    {
        InternalTypeForKind = NPCControl.ActionBehaviors.Disguise;
    }
}