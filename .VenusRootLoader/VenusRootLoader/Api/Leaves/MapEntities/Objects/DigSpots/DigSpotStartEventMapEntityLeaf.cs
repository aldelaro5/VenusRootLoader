using UnityEngine;
using VenusRootLoader.Registry;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.DigSpots;

public sealed class DigSpotStartEventMapEntityLeaf : DigSpotMapEntityLeaf
{
    internal DigSpotStartEventMapEntityLeaf(int gameId, string creatorId, string namedId)
        : base(gameId, creatorId, namedId)
    {
    }

    public Branch<EventLeaf> EventToStartWhenEmergingFromDigging
    {
        get;
        set
        {
            InternalData[1].Value = value.Resolve().GameId;
            field = value;
        }
    }

    [MapEntityInitializeFromNew]
    internal void InitializeFromNew(Vector3 startingPosition, Branch<EventLeaf> eventToStartWhenEmergingFromDigging)
    {
        base.InitializeFromNew(startingPosition);
        InternalData.AddRange([new(2), new(-1), new(-1)]);
        EventToStartWhenEmergingFromDigging = eventToStartWhenEmergingFromDigging;
    }

    internal override void InitializeFromExisting()
    {
        base.InitializeFromExisting();
        ILeavesRegistry<EventLeaf> eventsRegistry = RegistryResolver.Resolve<EventLeaf>();
        EventToStartWhenEmergingFromDigging = new(eventsRegistry.GetByGameId(InternalData[1].Value));
    }
}