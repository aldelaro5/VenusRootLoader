using UnityEngine;
using VenusRootLoader.Registry;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves.MapEntities.Npcs;

public sealed class EventNpcMapEntityLeaf : NpcWithSpyDialogueMapEntityLeaf
{
    internal EventNpcMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    private const int LockedDoorInteractionEventId = 59;

    internal override NPCControl.Interaction Interaction =>
        EventToStartWhenInteracting.GameId == LockedDoorInteractionEventId
            ? NPCControl.Interaction.LockedDoor
            : NPCControl.Interaction.Event;

    public Branch<EventLeaf> EventToStartWhenInteracting
    {
        get;
        set
        {
            InternalEventId = value.GameId;
            field = value;
        }
    }

    [MapEntityInitializeFromNew]
    internal void InitializeFromNew(
        Vector3 startingPosition,
        Branch<AnimIdLeaf>? animId,
        Branch<DialogueLeaf>? spyDialogue,
        Branch<EventLeaf> eventToStartWhenInteracting)
    {
        base.InitializeFromNew(startingPosition, animId, spyDialogue);
        EventToStartWhenInteracting = eventToStartWhenInteracting;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        base.InitializeFromExisting(registryResolver);
        ILeavesRegistry<EventLeaf> eventsRegistry = registryResolver.Resolve<EventLeaf>();

        EventToStartWhenInteracting = OriginalInteraction == NPCControl.Interaction.LockedDoor
            ? new(eventsRegistry.LeavesByGameIds[LockedDoorInteractionEventId])
            : new(eventsRegistry.LeavesByGameIds[InternalEventId]);
    }
}