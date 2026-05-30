using CommunityToolkit.Diagnostics;
using System.Collections.ObjectModel;
using UnityEngine;
using VenusRootLoader.Api.Leaves.MapEntities.ActionBehaviors.Enums;

namespace VenusRootLoader.Api.Leaves.MapEntities.ActionBehaviors;

public sealed class MoveAlongPathActionBehavior : ActionBehavior
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

    public ReadOnlyCollection<Vector3> MovementPathNodePositions { get; private set; } =
        new List<Vector3>().AsReadOnly();

    internal MoveAlongPathActionBehavior(MapEntityLeaf mapEntityLeaf, ActionBehaviorKind kind) :
        base(mapEntityLeaf, kind)
    {
    }

    public void ChangeMovementPathNodePositions(ICollection<Vector3> nodes)
    {
        MapEntityLeaf.InternalSecondaryVectorData.Clear();
        MapEntityLeaf.InternalSecondaryVectorData.AddRange(nodes);
        MovementPathNodePositions = nodes.ToList().AsReadOnly();
    }
}