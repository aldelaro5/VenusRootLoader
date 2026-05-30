using CommunityToolkit.Diagnostics;
using System.Collections.ObjectModel;
using UnityEngine;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects;

// TODO: Fix the length 0 issue on NPCControl.CreateEntities so this can move without a switch
public sealed class MovingPlatformAlongPathMapEntityLeaf : MapEntityLeaf
{
    internal MovingPlatformAlongPathMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.PathPlatform;
    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

    public Vector3 EulerAngles { get => InternalEulerAngles; set => InternalEulerAngles = value; }

    public Branch<AnimIdLeaf> AnimId
    {
        get;
        set
        {
            InternalAnimIdOrItemId = value.GameId;
            field = value;
        }
    }

    public ReadOnlyCollection<Branch<MapEntityLeaf>> RequiredEntityActivationsToMove { get; private set; } =
        new List<Branch<MapEntityLeaf>>().AsReadOnly();

    public ReadOnlyCollection<Vector3> MovementPathNodePositions { get; private set; } =
        new List<Vector3>().AsReadOnly();

    public int StartMovementFromNodeIndex
    {
        get => (int)InternalDialogues[0].x;
        set
        {
            Guard.IsBetweenOrEqualTo(value, 0, InternalVectorData.Count - 1, nameof(StartMovementFromNodeIndex));
            InternalDialogues[0] = new Vector3(value, InternalDialogues[0].y, InternalDialogues[0].z);
        }
    }

    public float MovementSpeedMultiplier
    {
        get => InternalDialogues[0].y;
        set => InternalDialogues[0] = new Vector3(InternalDialogues[0].x, value, InternalDialogues[0].z);
    }

    public float FramesDelayBeforeReversingWhenGoingInactiveAtNode
    {
        get => InternalDialogues[1].y;
        set => InternalDialogues[1] = new Vector3(InternalDialogues[1].x, value, InternalDialogues[1].z);
    }

    public float? ModelScale
    {
        get => InternalDialogues[2].x <= 0.1f ? null : InternalDialogues[2].x / 10f;
        set => InternalDialogues[2] = new Vector3(
            value is > 0.1f ? value.Value * 10f : 0f,
            InternalDialogues[2].y,
            InternalDialogues[2].z);
    }

    public float? FramesBeforeShockIfElectroPlatformOverride
    {
        get => InternalDialogues[2].y == 0f ? null : InternalDialogues[2].y;
        set => InternalDialogues[2] = new Vector3(InternalDialogues[2].x, value ?? 0f, InternalDialogues[2].z);
    }

    // TODO: Figure out a way to assign the actual AnimId branch here
    internal override void InitializeFromNew()
    {
        InternalDialogues.AddRange([new(0f, 5f, 0f), new(0f, 30f, 0f), new(0f, 0f, 0f)]);
        ChangeMovementPathNodePositions([Vector3.zero, Vector3.up]);
        InternalAnimIdOrItemId = (int)MainManager.AnimIDs.AncientPlatform - 1;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalDialogues.Count < 3)
            InternalDialogues.AddRange(Enumerable.Repeat(Vector3.zero, 3 - InternalDialogues.Count));

        ILeavesRegistry<AnimIdLeaf> animIdsRegistry = registryResolver.Resolve<AnimIdLeaf>();
        AnimId = new(animIdsRegistry.LeavesByGameIds[InternalAnimIdOrItemId]);

        List<int> requiredEntityIdsActivation = new();
        requiredEntityIdsActivation.AddRange(InternalData);
        RequiredEntityActivationsToMove = requiredEntityIdsActivation
            .Select(x => new Branch<MapEntityLeaf>(Map.Leaf.EntitiesRegistry.LeavesByGameIds[x])).ToList().AsReadOnly();

        List<Vector3> movementPathNodePositions = new();
        movementPathNodePositions.AddRange(InternalVectorData);
        MovementPathNodePositions = movementPathNodePositions.AsReadOnly();
    }

    public void ChangeRequiredEntityActivationsToMove(List<Branch<MapEntityLeaf>> entities)
    {
        List<string> badMapEntitiesNamedIds = entities
            .Where(x => x.Leaf.Map != Map)
            .Select(x => x.NamedId)
            .ToList();
        if (badMapEntitiesNamedIds.Count > 0)
        {
            ThrowHelper.ThrowArgumentException(
                nameof(entities),
                $"The following map entities needs to be in the {Map.NamedId} map, but they are not: " +
                $"{string.Join(", ", badMapEntitiesNamedIds)}");
        }

        InternalData.Clear();
        InternalData.AddRange(entities.Select(e => e.GameId));
        RequiredEntityActivationsToMove = entities.AsReadOnly();
    }

    public void ChangeMovementPathNodePositions(List<Vector3> nodes)
    {
        InternalVectorData.Clear();
        InternalVectorData.AddRange(nodes);
        MovementPathNodePositions = nodes.AsReadOnly();
        InternalStartingPosition = nodes[0];
    }
}