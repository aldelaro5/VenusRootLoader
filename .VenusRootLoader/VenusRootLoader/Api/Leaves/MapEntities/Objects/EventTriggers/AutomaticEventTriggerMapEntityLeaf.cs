using VenusRootLoader.Registry;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.EventTriggers;

public sealed class AutomaticEventTriggerMapEntityLeaf : EventTriggerMapEntityLeaf
{
    internal AutomaticEventTriggerMapEntityLeaf(int gameId, string creatorId, string namedId)
        : base(gameId, creatorId, namedId)
    {
    }

    public Branch<EventLeaf> EventToImmediatelyStartOnMapLoad
    {
        get;
        set
        {
            InternalData[0].Value = value.Resolve().GameId;
            field = value;
        }
    } = null!;

    [MapEntityInitializeFromNew]
    internal void InitializeFromNew(Branch<EventLeaf> eventToImmediatelyStartOnMapLoad)
    {
        InternalData.AddRange([new(-1), new(0), new(1)]);
        EventToImmediatelyStartOnMapLoad = eventToImmediatelyStartOnMapLoad;
        EntityStartingPosition = new(0f, -999f, 0f);
    }

    internal override void InitializeFromExisting()
    {
        ILeavesRegistry<EventLeaf> eventsRegistry = RegistryResolver.Resolve<EventLeaf>();
        EventToImmediatelyStartOnMapLoad = new(eventsRegistry.GetByGameId(InternalData[0].Value));
    }
}