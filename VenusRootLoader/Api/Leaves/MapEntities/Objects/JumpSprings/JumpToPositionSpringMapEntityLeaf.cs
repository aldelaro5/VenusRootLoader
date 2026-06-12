using CommunityToolkit.Diagnostics;
using UnityEngine;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.JumpSprings;

public sealed class JumpToPositionSpringMapEntityLeaf : JumpSpringMapEntityLeaf
{
    internal JumpToPositionSpringMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    public Branch<InsideTransitionZoneMapEntityLeaf>? InsideTransitionToTriggerWhenUsingSpring
    {
        get;
        set
        {
            if (value is not null && value.Value.Leaf.Map != Map)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(
                    nameof(InsideTransitionToTriggerWhenUsingSpring),
                    $"The entity is not in the {Map.NamedId} map which is required");
            }

            InternalData[2].Value = value?.GameId ?? -1;
            field = value;
        }
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

    internal void InitializeFromNew(Vector3 startingPosition, Vector3 positionToGoWhenUsingSpring)
    {
        base.InitializeFromNew(startingPosition);
        InternalData.AddRange([new(1), new(0), new(-1)]);
        InternalVectorData.AddRange([new(new(15f, 0f, 0f)), new(Vector3.zero), new(Vector3.right)]);
        PositionToGoWhenUsingSpring = positionToGoWhenUsingSpring;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalVectorData.Count < 3)
            InternalVectorData.Add(new(Vector3.right));

        if (InternalData[2].Value >= 0)
        {
            InsideTransitionToTriggerWhenUsingSpring =
                Map.Leaf.EntitiesRegistry.LeavesByGameIds[InternalData[2].Value] is InsideTransitionZoneMapEntityLeaf
                    insideTransition
                    ? new(insideTransition)
                    : null;
        }
    }
}