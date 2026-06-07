using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.EventTriggers;

public sealed class AutomaticEventTriggerMapEntityLeaf : EventTriggerMapEntityLeaf
{
    internal AutomaticEventTriggerMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    public Branch<EventLeaf> EventToImmediatelyStartOnMapLoad
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
        EventToImmediatelyStartOnMapLoad = new(eventsRegistry.LeavesByGameIds[InternalData[0].Value]);
    }
}