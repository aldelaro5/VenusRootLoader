using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Npcs;

public sealed class EventNpcMapEntityLeaf : SpyableNpcMapEntityLeaf
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

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        base.InitializeFromExisting(registryResolver);
        ILeavesRegistry<EventLeaf> eventsRegistry = registryResolver.Resolve<EventLeaf>();

        EventToStartWhenInteracting = OriginalInteraction == NPCControl.Interaction.LockedDoor
            ? new(eventsRegistry.LeavesByGameIds[LockedDoorInteractionEventId])
            : new(eventsRegistry.LeavesByGameIds[InternalEventId]);
    }
}