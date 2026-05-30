using System.Collections.ObjectModel;
using UnityEngine;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects;

// TODO: Fix the length 0 issue on NPCControl.CreateEntities so this can move without a switch
public sealed class MovingPlatformAlongLerpMapEntityLeaf : MapEntityLeaf
{
    internal MovingPlatformAlongLerpMapEntityLeaf(int gameId, string namedId, string creatorId)
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

    public Vector3 InactivePosition
    {
        get => InternalVectorData[0];
        set
        {
            InternalVectorData[0] = value;
            InternalStartingPosition = value;
        }
    }

    public Vector3 ActivePosition { get => InternalVectorData[1]; set => InternalVectorData[1] = value; }

    public ReadOnlyCollection<int> RequiredEntityActivationsToMove { get; private set; } =
        new List<int>().AsReadOnly();

    public bool StartMovementFromActivePosition
    {
        get => (int)InternalDialogues[0].x == 1;
        set => InternalDialogues[0] = new Vector3(value ? 1f : 0f, InternalDialogues[0].y, InternalDialogues[0].z);
    }

    public float MovementSpeedMultiplier
    {
        get => InternalDialogues[0].y;
        set => InternalDialogues[0] = new Vector3(InternalDialogues[0].x, value, InternalDialogues[0].z);
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

    internal override void InitializeFromNew()
    {
        InternalDialogues.AddRange([new(0f, 5f, 0f), new(1f, 0f, 0f), new(0f, 0f, 0f)]);
        InternalVectorData.AddRange([Vector3.zero, Vector3.up]);
        InternalAnimIdOrItemId = (int)MainManager.AnimIDs.AncientPlatform - 1;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalDialogues.Count < 3)
            InternalDialogues.AddRange(Enumerable.Repeat(Vector3.zero, 3 - InternalData.Count));

        ILeavesRegistry<AnimIdLeaf> animIdsRegistry = registryResolver.Resolve<AnimIdLeaf>();
        AnimId = new(animIdsRegistry.LeavesByGameIds[InternalAnimIdOrItemId]);

        List<int> requiredEntityIdsActivation = new();
        requiredEntityIdsActivation.AddRange(InternalData);
        RequiredEntityActivationsToMove = requiredEntityIdsActivation.AsReadOnly();
    }

    public void ChangeRequiredEntityActivationsToMove(List<int> entityIds)
    {
        InternalData.Clear();
        InternalData.AddRange(entityIds);
        RequiredEntityActivationsToMove = entityIds.AsReadOnly();
    }
}