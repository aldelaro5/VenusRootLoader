using UnityEngine;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.Crystals;

public sealed class DeadLanderOmegaAlertCrystalMapEntityLeaf : CrystalMapEntityLeaf
{
    internal DeadLanderOmegaAlertCrystalMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    public int DeadLanderOmegaId { get => InternalData[1].Value - 10; set => InternalData[1].Value = value + 10; }

    public Vector3 PositionDeadLanderOmegaLooksAtWhenHit
    {
        get => InternalVectorData[0].Value;
        set => InternalVectorData[0].Value = value;
    }

    [MapEntityInitializeFromNew]
    internal void InitializeFromNew(
        Vector3 startingPosition,
        int deadLanderOmegaId,
        Vector3 positionDeadLanderOmegaLooksAtWhenHit)
    {
        base.InitializeFromNew(startingPosition);
        InternalData.AddRange([new(1), new(10), new(0)]);
        DeadLanderOmegaId = deadLanderOmegaId;
        PositionDeadLanderOmegaLooksAtWhenHit = positionDeadLanderOmegaLooksAtWhenHit;
    }
}