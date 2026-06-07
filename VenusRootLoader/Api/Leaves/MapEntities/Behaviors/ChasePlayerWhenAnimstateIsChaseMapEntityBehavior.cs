using VenusRootLoader.Api.Leaves.MapEntities.Behaviors.Enums;

namespace VenusRootLoader.Api.Leaves.MapEntities.Behaviors;

public sealed class ChasePlayerWhenAnimstateIsChaseMapEntityBehavior : MapEntityBehavior
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

    internal ChasePlayerWhenAnimstateIsChaseMapEntityBehavior(MapEntityLeaf mapEntityLeaf, ActionBehaviorKind kind)
        : base(mapEntityLeaf, kind)
    {
        InternalTypeForKind = NPCControl.ActionBehaviors.ChaseWhenAnim;
    }
}