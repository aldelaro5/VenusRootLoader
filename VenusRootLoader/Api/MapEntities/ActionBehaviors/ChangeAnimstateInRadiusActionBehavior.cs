namespace VenusRootLoader.Api.MapEntities.ActionBehaviors;

public sealed class ChangeAnimstateInRadiusActionBehavior : ActionBehavior
{
    public float Radius
    {
        get => MapEntity.InternalWanderRadius;
        set => MapEntity.InternalWanderRadius = value;
    }

    public int AnimstateWhenOutsideRadius
    {
        get => (int)MapEntity.InternalPrimaryActionFrequency;
        set => MapEntity.InternalPrimaryActionFrequency = value;
    }

    public int AnimstateWhenInsideRadius
    {
        get => (int)MapEntity.InternalSecondaryActionFrequency;
        set => MapEntity.InternalSecondaryActionFrequency = value;
    }

    internal ChangeAnimstateInRadiusActionBehavior(MapEntity mapEntity) : base(mapEntity, null)
    {
        mapEntity.InternalPrimaryBehavior = NPCControl.ActionBehaviors.ChangeSpriteInRandius;
        mapEntity.InternalSecondaryBehavior = NPCControl.ActionBehaviors.ChangeSpriteInRandius;
    }
}