using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects;

// TODO: Fix the length 0 issue on NPCControl.CreateEntities so this can move without a switch
public sealed class MovingPlatformAlongLerpMapEntityLeaf : MapEntityLeaf
{
    internal MovingPlatformAlongLerpMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
        _requiredEntityActivationsToMove = new(InternalData, 0, x => new(x.GameId));
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
        get => InternalVectorData[0].Value;
        set
        {
            InternalVectorData[0].Value = value;
            InternalStartingPosition = value;
        }
    }

    public Vector3 ActivePosition { get => InternalVectorData[1].Value; set => InternalVectorData[1].Value = value; }

    private readonly ListRefWrapper<Branch<MapEntityLeaf>, int> _requiredEntityActivationsToMove;
    public IList<Branch<MapEntityLeaf>> RequiredEntityActivationsToMove => _requiredEntityActivationsToMove;

    public bool StartMovementFromActivePosition
    {
        get => (int)InternalDialogues[0].Value.x == 1;
        set => InternalDialogues[0].Value.x = value ? 1f : 0f;
    }

    public float MovementSpeedMultiplier
    {
        get => InternalDialogues[0].Value.y;
        set => InternalDialogues[0].Value.y = value;
    }

    public float? ModelScale
    {
        get => InternalDialogues[2].Value.x <= 0.1f ? null : InternalDialogues[2].Value.x / 10f;
        set => InternalDialogues[2].Value.x = value is > 0.1f ? value.Value * 10f : 0f;
    }

    public float? FramesBeforeShockIfElectroPlatformOverride
    {
        get => InternalDialogues[2].Value.y == 0f ? null : InternalDialogues[2].Value.y;
        set => InternalDialogues[2].Value.y = value ?? 0f;
    }

    internal override void InitializeFromNew()
    {
        InternalDialogues.AddRange([new(new(0f, 5f, 0f)), new(new(1f, 0f, 0f)), new(new(0f, 0f, 0f))]);
        InternalVectorData.AddRange([new(Vector3.zero), new(Vector3.up)]);
        InternalAnimIdOrItemId = (int)MainManager.AnimIDs.AncientPlatform - 1;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalDialogues.Count < 3)
            InternalDialogues.AddRange(Enumerable.Repeat(new Ref<Vector3>(Vector3.zero), 3 - InternalData.Count));

        ILeavesRegistry<AnimIdLeaf> animIdsRegistry = registryResolver.Resolve<AnimIdLeaf>();
        AnimId = new(animIdsRegistry.LeavesByGameIds[InternalAnimIdOrItemId]);

        _requiredEntityActivationsToMove.SynchronizeFromExistingData(
            InternalData
                .Select(x => new Branch<MapEntityLeaf>(Map.Leaf.EntitiesRegistry.LeavesByGameIds[x.Value]))
                .ToList());
    }
}