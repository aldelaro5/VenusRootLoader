using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.MapEntities.ActionBehaviors;

public sealed class FacePlayerActionBehavior : ActionBehavior
{
    internal override NPCControl.ActionBehaviors BehaviorType => NPCControl.ActionBehaviors.FacePlayer;

    internal FacePlayerActionBehavior(MapEntity mapEntity, ActionBehaviorKind kind) : base(mapEntity, kind) { }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver) { }
}