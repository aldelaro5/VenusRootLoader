using UnityEngine;

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

    internal override void InitializeFromNew()
    {
        base.InitializeFromNew();
        InternalData.AddRange([new(1), new(10), new(0)]);
    }
}