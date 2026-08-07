using UnityEngine;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.SetRespawnZones;

public sealed class IndependantRespawnZoneMapEntityLeaf : ObjectMapEntityLeaf
{
    internal IndependantRespawnZoneMapEntityLeaf(int gameId, string creatorId, string namedId)
        : base(gameId, creatorId, namedId)
    {
    }

    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.SetPlayerRespawn;

    [MapEntityInitializeFromNew]
    internal void InitializeFromNew(Vector3 startingPosition)
    {
        EntityStartingPosition = startingPosition;
        InternalVectorData.Add(new(Vector3.zero));
    }

    internal override void InitializeFromExisting()
    {
        if (InternalVectorData.Count == 0)
            InternalVectorData.Add(new(Vector3.zero));
    }
}