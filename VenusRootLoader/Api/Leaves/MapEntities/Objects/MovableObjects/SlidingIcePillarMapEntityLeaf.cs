using UnityEngine;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.MovableObjects;

public sealed class SlidingIcePillarMapEntityLeaf : MovableObjectMapEntityLeaf
{
    internal SlidingIcePillarMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    public float SlidingVelocityMultiplier
    {
        get => InternalVectorData[0].Value.z;
        set => InternalVectorData[0].Value.z = value;
    }

    public Vector3? IcePillarScaleOverride
    {
        get => InternalBoxColSize.magnitude <= 0.1f ? null : InternalBoxColSize;
        set => InternalBoxColSize = value is null || value.Value.magnitude <= 0.1f ? Vector3.zero : value.Value;
    }

    internal override void InitializeFromNew()
    {
        base.InitializeFromNew();
        InternalData.AddRange([new(0), new(0), new(3), new(0)]);
        InternalVectorData.AddRange([new(new(0f, 0f, 0.1f)), new(new(0f, 0f, 0f))]);
        InternalBoxColSize = Vector3.zero;
        InternalAnimIdOrItemId = (int)MainManager.AnimIDs.IcePillarObj - 1;
    }
}