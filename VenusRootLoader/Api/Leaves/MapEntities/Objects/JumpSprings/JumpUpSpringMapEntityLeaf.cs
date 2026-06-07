using UnityEngine;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.JumpSprings;

public sealed class JumpUpSpringMapEntityLeaf : JumpSpringMapEntityLeaf
{
    internal JumpUpSpringMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    public float? JumpHeightOverrideWhenUsingSpring
    {
        get => InternalVectorData[0].Value.x <= 1.0f ? null : InternalVectorData[0].Value.x;
        set => InternalVectorData[0].Value.x = value ?? 0f;
    }

    internal override void InitializeFromNew()
    {
        base.InitializeFromNew();
        InternalData.AddRange([new(0), new(0), new(-1)]);
        InternalVectorData.AddRange([new(new(15f, 0f, 0f)), new(Vector3.zero), new(Vector3.zero)]);
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver) { }
}