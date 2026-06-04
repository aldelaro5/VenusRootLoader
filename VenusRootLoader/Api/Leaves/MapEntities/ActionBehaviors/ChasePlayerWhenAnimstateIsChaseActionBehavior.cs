using VenusRootLoader.Api.Leaves.MapEntities.ActionBehaviors.Enums;

namespace VenusRootLoader.Api.Leaves.MapEntities.ActionBehaviors;

public sealed class ChasePlayerWhenAnimstateIsChaseActionBehavior : ActionBehavior
{
    public int AnimstateOverrideWhenNotChase
    {
        get => (int)InternalFrequencyForKind;
        set => InternalFrequencyForKind = value;
    }

    public float MovementSpeedMultiplier
    {
        get => MapEntityLeaf.InternalSpeedMultiplier;
        set => MapEntityLeaf.InternalSpeedMultiplier = value;
    }

    internal ChasePlayerWhenAnimstateIsChaseActionBehavior(MapEntityLeaf mapEntityLeaf, ActionBehaviorKind kind)
        : base(mapEntityLeaf, kind)
    {
        InternalTypeForKind = NPCControl.ActionBehaviors.ChaseWhenAnim;
    }
}