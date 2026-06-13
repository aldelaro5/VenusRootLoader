using UnityEngine;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.RollingRocks;

public sealed class RollingRockWithoutCanonMapEntityLeaf : RollingRockMapEntityLeaf
{
    internal RollingRockWithoutCanonMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    public RollingRockMethod RollingMethod
    {
        get => (RollingRockMethod)InternalData[0].Value;
        set => InternalData[0].Value = (int)value;
    }

    [MapEntityInitializeFromNew]
    internal void InitializeFromNew(
        Vector3 startingPosition,
        Vector3 destinationPosition,
        RollingRockMethod rollingMethod)
    {
        base.InitializeFromNew(startingPosition, destinationPosition);
        InternalData.AddRange([new(1), new(0), new(0), new(-1)]);
        InternalVectorData.AddRange([new(new(10f, 0f, 0f)), new(new(-10f, 0f, 0f)), new(new(0f, 0f, 5f))]);
        RollingMethod = rollingMethod;
    }
}