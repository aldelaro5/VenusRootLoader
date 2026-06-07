using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.EventTriggers;

public sealed class AutomaticEventTriggerMapEntityLeaf : MapEntityLeaf
{
    internal AutomaticEventTriggerMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.EventTrigger;
    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

    public Branch<EventLeaf> EventToImmediatelyStart
    {
        get;
        set
        {
            InternalData[0].Value = value.GameId;
            field = value;
        }
    }

    internal override void InitializeFromNew()
    {
        InternalData.AddRange([new(-1), new(0), new(1)]);
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        ILeavesRegistry<EventLeaf> eventsRegistry = registryResolver.Resolve<EventLeaf>();
        EventToImmediatelyStart = new(eventsRegistry.LeavesByGameIds[InternalData[0].Value]);
    }
}