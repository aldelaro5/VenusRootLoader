using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Npcs;

public sealed class EventNpcMapEntityLeaf : NpcMapEntityLeaf
{
    internal EventNpcMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    private const int LockedDoorInteractionEventId = 59;

    internal override NPCControl.Interaction Interaction =>
        EventToStartOnInteract.GameId == LockedDoorInteractionEventId
            ? NPCControl.Interaction.LockedDoor
            : NPCControl.Interaction.Event;

    public Branch<EventLeaf> EventToStartOnInteract
    {
        get;
        set
        {
            InternalEventId = value.GameId;
            field = value;
        }
    }

    public Branch<DialogueLeaf>? SpyDialogue
    {
        get;
        set
        {
            InternalSpyDialogueId = value?.GameId ?? -1;
            field = value;
        }
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        base.InitializeFromExisting(registryResolver);
        ILeavesRegistry<EventLeaf> eventsRegistry = registryResolver.Resolve<EventLeaf>();
        ILeavesRegistry<CommonDialogueLeaf> commonDialoguesRegistry = registryResolver.Resolve<CommonDialogueLeaf>();

        if (InternalSpyDialogueId != -1)
        {
            SpyDialogue = InternalSpyDialogueId < 0
                ? commonDialoguesRegistry.LeavesByGameIds[InternalSpyDialogueId]
                : Map.Leaf.DialoguesRegistry.LeavesByGameIds[InternalSpyDialogueId];
        }

        EventToStartOnInteract = OriginalInteraction == NPCControl.Interaction.LockedDoor
            ? new(eventsRegistry.LeavesByGameIds[LockedDoorInteractionEventId])
            : new(eventsRegistry.LeavesByGameIds[InternalEventId]);
    }
}