using UnityEngine;
using VenusRootLoader.Registry;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.SetRespawnZones;

public sealed class IndependantRespawnZoneMapEntityLeaf : ObjectMapEntityLeaf
{
    internal IndependantRespawnZoneMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.SetPlayerRespawn;

    [MapEntityInitializeFromNew]
    internal void InitializeFromNew(Vector3 startingPosition)
    {
        EntityStartingPosition = startingPosition;
        InternalVectorData.Add(new(Vector3.zero));
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalVectorData.Count == 0)
            InternalVectorData.Add(new(Vector3.zero));
    }
}