using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.MovableObjects;

public sealed class MovableRockMapEntityLeaf : MovableObjectMapEntityLeaf
{
    internal MovableRockMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    public float LaunchVerticalVelocity
    {
        get => InternalVectorData[0].Value.y;
        set => InternalVectorData[0].Value.y = value;
    }

    public float LaunchHorizontalVelocityMultiplier
    {
        get => InternalVectorData[0].Value.z;
        set => InternalVectorData[0].Value.z = value;
    }

    [MapEntityInitializeFromNew]
    internal override void InitializeFromNew(Vector3 startingPosition)
    {
        base.InitializeFromNew(startingPosition);
        InternalData.AddRange(Enumerable.Repeat(new Ref<int>(0), 4));
        InternalVectorData.AddRange([new(new(0f, 10f, 5f)), new(new(0f, 0f, 0f))]);
        InternalAnimIdOrItemId = (int)MainManager.AnimIDs.PushRock - 1;
    }
}