using UnityEngine;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.JumpSprings;

public sealed class JumpToPositionSpringMapEntityLeaf : JumpSpringMapEntityLeaf
{
    internal JumpToPositionSpringMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    // TODO: Type this correctly
    public int? InsideTransitionMapEntityIdToTriggerWhenUsingSpring
    {
        get => InternalData[2].Value < 0 ? null : InternalData[2].Value;
        set => InternalData[2].Value = value ?? -1;
    }

    public Vector3 PositionToGoWhenUsingSpring
    {
        get => InternalVectorData[1].Value;
        set => InternalVectorData[1].Value = value;
    }

    public float JumpHeightWhenUsingSpring
    {
        get => InternalVectorData[0].Value.x;
        set => InternalVectorData[0].Value.x = value;
    }

    public float JumpDurationDivisorWhenUsingSpring
    {
        get => Mathf.Clamp(InternalVectorData[2].Value.x, 1f, 99f);
        set => InternalVectorData[2].Value.x = Mathf.Clamp(value, 1f, 99f);
    }

    internal override void InitializeFromNew()
    {
        base.InitializeFromNew();
        InternalData.AddRange([new(1), new(0), new(-1)]);
        InternalVectorData.AddRange([new(new(15f, 0f, 0f)), new(Vector3.zero), new(Vector3.right)]);
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalVectorData.Count < 3)
            InternalVectorData.Add(new(Vector3.right));
    }
}