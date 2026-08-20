using CommunityToolkit.Diagnostics;
using UnityEngine;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.MovingPlatforms;

public sealed class MovingPlatformAlongLerpMapEntityLeaf : MovingPlatformMapEntityLeaf
{
    internal MovingPlatformAlongLerpMapEntityLeaf(int gameId, string creatorId, string namedId)
        : base(gameId, creatorId, namedId)
    {
    }

    public override Vector3 EntityStartingPosition
    {
        get => base.EntityStartingPosition;
        set
        {
            base.EntityStartingPosition = value;
            InternalVectorData[0].Value = value;
        }
    }

    public Vector3 ActivePositionToMoveTowards
    {
        get => InternalVectorData[1].Value;
        set => InternalVectorData[1].Value = value;
    }

    public bool StartMovementFromActivePosition
    {
        get => (int)InternalDialogues[0].Value.x == 1;
        set => InternalDialogues[0].Value.x = value ? 1f : 0f;
    }

    [MapEntityInitializeFromNew]
    internal void InitializeFromNew(
        Vector3 startingPosition,
        Branch<AnimIdLeaf> animId,
        IList<Branch<ObjectMapEntityLeaf>> requiredEntityActivationsToMove,
        Vector3 activePositionToMoveTowards)
    {
        Guard.IsNotEmpty(requiredEntityActivationsToMove);
        base.InitializeFromNew(startingPosition, animId, requiredEntityActivationsToMove);
        InternalVectorData.AddRange([new(startingPosition), new(activePositionToMoveTowards)]);
        InternalDialogues.AddRange([new(new(0f, 5f, 0f)), new(new(1f, 0f, 0f)), new(new(0f, 0f, 0f))]);
    }
}