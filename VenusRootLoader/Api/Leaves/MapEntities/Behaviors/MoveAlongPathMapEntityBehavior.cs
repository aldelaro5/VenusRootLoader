using CommunityToolkit.Diagnostics;
using UnityEngine;
using VenusRootLoader.Api.Leaves.MapEntities.Behaviors.Enums;
using VenusRootLoader.LeavesInternals;

namespace VenusRootLoader.Api.Leaves.MapEntities.Behaviors;

public sealed class MoveAlongPathMapEntityBehavior : MapEntityBehavior
{
    public bool JumpWhileMoving
    {
        get => InternalTypeForKind switch
        {
            NPCControl.ActionBehaviors.SetPath => false,
            NPCControl.ActionBehaviors.SetPathJump => true,
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<bool>(nameof(InternalTypeForKind))
        };
        set => InternalTypeForKind = value
            ? NPCControl.ActionBehaviors.SetPathJump
            : NPCControl.ActionBehaviors.SetPath;
    }

    public float DelayFramesBeforeMovingToNextNode
    {
        get => InternalFrequencyForKind;
        set => InternalFrequencyForKind = value;
    }

    private readonly ListRefWrapper<Vector3, Vector3> _movementPathNodePositions;
    public IList<Vector3> MovementPathNodePositions => _movementPathNodePositions;

    internal MoveAlongPathMapEntityBehavior(MapEntityLeaf mapEntityLeaf, ActionBehaviorKind kind) :
        base(mapEntityLeaf, kind)
    {
        _movementPathNodePositions = new(MapEntityLeaf.InternalSecondaryVectorData, 0, x => new(x));
        _movementPathNodePositions.SynchronizeFromExistingData(
            MapEntityLeaf.InternalSecondaryVectorData.Select(x => x.Value).ToList());
    }
}