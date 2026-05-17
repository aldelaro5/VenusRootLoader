using CommunityToolkit.Diagnostics;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.MapEntities.ActionBehaviors;

public sealed class BlankActionBehavior : ActionBehavior
{
    internal override NPCControl.ActionBehaviors BehaviorType => Kind switch
    {
        ActionBehaviorKind.OutOfRange => MapEntity.InternalPrimaryBehavior,
        ActionBehaviorKind.InRange => MapEntity.InternalSecondaryBehavior,
        _ => ThrowHelper.ThrowArgumentOutOfRangeException<NPCControl.ActionBehaviors>(nameof(Kind))
    };

    internal BlankActionBehavior(MapEntity mapEntity, ActionBehaviorKind kind) : base(mapEntity, kind) { }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver) { }
}