namespace VenusRootLoader.Api.Leaves.MapEntities.ActionBehaviors;

public sealed class ChangeAnimstateInRadiusActionBehavior : ActionBehavior
{
    public float Radius
    {
        get => MapEntityLeaf.InternalWanderRadius;
        set => MapEntityLeaf.InternalWanderRadius = value;
    }

    public int AnimstateWhenOutsideRadius
    {
        get => (int)MapEntityLeaf.InternalOutOfRangeActionFrequency;
        set => MapEntityLeaf.InternalOutOfRangeActionFrequency = value;
    }

    public int AnimstateWhenInsideRadius
    {
        get => (int)MapEntityLeaf.InternalInRangeActionFrequency;
        set => MapEntityLeaf.InternalInRangeActionFrequency = value;
    }

    internal ChangeAnimstateInRadiusActionBehavior(MapEntityLeaf mapEntityLeaf) : base(mapEntityLeaf, null)
    {
        mapEntityLeaf.InternalOutOfRangeBehavior = NPCControl.ActionBehaviors.ChangeSpriteInRandius;
        mapEntityLeaf.InternalInRangeBehavior = NPCControl.ActionBehaviors.ChangeSpriteInRandius;
    }
}