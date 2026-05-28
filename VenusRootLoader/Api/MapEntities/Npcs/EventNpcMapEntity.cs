using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Api.MapEntities.ActionBehaviors;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.MapEntities.Npcs;

public sealed class EventNpcMapEntity : MapEntity
{
    internal EventNpcMapEntity(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
        Behaviors = new(this);
    }

    private const int LockedDoorInteractionEventId = 59;

    internal override NPCControl.NPCType Type => NPCControl.NPCType.NPC;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.None;

    internal override NPCControl.Interaction Interaction =>
        EventToStartOnInteract.GameId == LockedDoorInteractionEventId
            ? NPCControl.Interaction.LockedDoor
            : NPCControl.Interaction.Event;

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

    public Branch<EventLeaf> EventToStartOnInteract
    {
        get;
        set
        {
            InternalEventId = value.GameId;
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
        ILeavesRegistry<EventLeaf> eventsRegistry = registryResolver.Resolve<EventLeaf>();

        Behaviors.InitializeBehaviorFromExisting(registryResolver);

        EventToStartOnInteract = OriginalInteraction == NPCControl.Interaction.LockedDoor
            ? new(eventsRegistry.LeavesByGameIds[LockedDoorInteractionEventId])
            : new(eventsRegistry.LeavesByGameIds[InternalEventId]);
        AnimId = new(animIdsRegistry.LeavesByGameIds[InternalAnimIdOrItemId]);
    }
}