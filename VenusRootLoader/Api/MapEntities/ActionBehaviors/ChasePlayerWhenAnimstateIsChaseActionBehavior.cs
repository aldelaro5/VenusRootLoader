using VenusRootLoader.Api.MapEntities.ActionBehaviors.Enums;

namespace VenusRootLoader.Api.MapEntities.ActionBehaviors;

public sealed class ChasePlayerWhenAnimstateIsChaseActionBehavior : ActionBehavior
{
    public int AnimstateOverrideWhenNotChase
    {
        get => (int)InternalFrequencyForKind;
        set => InternalFrequencyForKind = value;
    }

    internal ChasePlayerWhenAnimstateIsChaseActionBehavior(MapEntity mapEntity, ActionBehaviorKind kind)
        : base(mapEntity, kind)
    {
        InternalTypeForKind = NPCControl.ActionBehaviors.ChaseWhenAnim;
    }
}