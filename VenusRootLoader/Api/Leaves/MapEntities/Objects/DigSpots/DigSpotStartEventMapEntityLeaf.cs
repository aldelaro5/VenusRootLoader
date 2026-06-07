using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.DigSpots;

public sealed class DigSpotStartEventMapEntityLeaf : DigSpotMapEntityLeaf
{
    internal DigSpotStartEventMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    public Branch<EventLeaf> EventToStartWhenEmergingFromDigging
    {
        get;
        set
        {
            InternalData[1].Value = value.GameId;
            field = value;
        }
    }

    internal override void InitializeFromNew()
    {
        base.InitializeFromNew();
        InternalData.AddRange([new(2), new(-1), new(-1)]);
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        base.InitializeFromExisting(registryResolver);
        ILeavesRegistry<EventLeaf> eventsRegistry = registryResolver.Resolve<EventLeaf>();
        EventToStartWhenEmergingFromDigging = new(eventsRegistry.LeavesByGameIds[InternalData[1].Value]);
    }
}