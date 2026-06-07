using CommunityToolkit.Diagnostics;
using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.MovingPlatforms;

public sealed class MovingPlatformAlongPathMapEntityLeaf : MovingPlatformMapEntityLeaf
{
    internal MovingPlatformAlongPathMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
        _movementPathNodePositions = new(InternalVectorData, 0, x => new(x));
    }

    private readonly ListRefWrapper<Vector3, Vector3> _movementPathNodePositions;
    public IList<Vector3> MovementPathNodePositions => _movementPathNodePositions;

    public int StartMovementFromNodeIndex
    {
        get => (int)InternalDialogues[0].Value.x;
        set
        {
            Guard.IsBetweenOrEqualTo(value, 0, InternalVectorData.Count - 1, nameof(StartMovementFromNodeIndex));
            InternalDialogues[0].Value.x = value;
        }
    }

    public float FramesDelayBeforeReversingWhenGoingInactiveAtNode
    {
        get => InternalDialogues[1].Value.y;
        set => InternalDialogues[1].Value.y = value;
    }

    internal override void InitializeFromNew()
    {
        base.InitializeFromNew();
        InternalDialogues.AddRange([new(new(0f, 5f, 0f)), new(new(0f, 30f, 0f)), new(new(0f, 0f, 0f))]);
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        base.InitializeFromExisting(registryResolver);
        _movementPathNodePositions.SynchronizeFromExistingData(
            InternalVectorData.Select(x => x.Value)
                .ToList());
    }
}