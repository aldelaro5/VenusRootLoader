using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Api.MapEntities.ActionBehaviors;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.MapEntities.Npcs;

public sealed class MedalsShopMapEntity : MapEntity
{
    internal override NPCControl.NPCType Type => NPCControl.NPCType.NPC;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.None;
    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.Shop;

    public Vector3 StartingPosition { get => InternalStartingPosition; set => InternalStartingPosition = value; }

    public Branch<AnimIdLeaf> AnimId
    {
        get;
        set
        {
            InternalAnimIdOrItemId = value.GameId;
            field = value;
        }
    }

    public float MovementRadius
    {
        get => InternalRadiusLimit;
        set => InternalRadiusLimit = value;
    }

    public float BehaviorAndInteractRangeRadius
    {
        get => InternalRadius;
        set => InternalRadius = value;
    }

    public MapEntityBehaviors Behaviors { get; }

    internal MedalsShopMapEntity()
    {
        Behaviors = new(this);
    }

    internal override void InitializeFromNew()
    {
        InternalAnimIdOrItemId = 0;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        ILeavesRegistry<FlagLeaf> flagsRegistry = registryResolver.Resolve<FlagLeaf>();
        ILeavesRegistry<AnimIdLeaf> animIdsRegistry = registryResolver.Resolve<AnimIdLeaf>();

        Behaviors.InitializeBehaviorFromExisting(registryResolver);

        AnimId = new(animIdsRegistry.LeavesByGameIds[InternalAnimIdOrItemId]);
    }
}