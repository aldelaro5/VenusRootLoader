using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Api.MapEntities.ActionBehaviors;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.MapEntities.Npcs;

public sealed class VenusHealingNpcMapEntity : MapEntity
{
    internal VenusHealingNpcMapEntity(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
        Behaviors = new(this);
    }

    internal override NPCControl.NPCType Type => NPCControl.NPCType.NPC;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.None;
    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.VenusHeal;

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

    public float BehaviorAndInteractRangeRadius
    {
        get => InternalRadius;
        set => InternalRadius = value;
    }

    public MapEntityBehaviors Behaviors { get; }

    internal override void InitializeFromNew() { }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        ILeavesRegistry<AnimIdLeaf> animIdsRegistry = registryResolver.Resolve<AnimIdLeaf>();

        Behaviors.InitializeBehaviorFromExisting(registryResolver);

        AnimId = new(animIdsRegistry.LeavesByGameIds[InternalAnimIdOrItemId]);
    }
}