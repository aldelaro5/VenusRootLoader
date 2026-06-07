using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects;

// TODO: Fix the length 0 issue on NPCControl.CreateEntities so this can move without a switch
// TODO: Fix a Rotater issue where its eulerAngles can get permanently stuck while on a platform rotating in x/z, here's a simple reproduction:
// rotate.ChangeMovementPathNodeEulerAngles(
// [
//     new(-90f, 0f, 0f),
//     new(-90f, 90f, 0f),
//     new(-60f, 0f, 30f)
// ]);
public sealed class RotatingPlatformMapEntityLeaf : ObjectMapEntityLeaf
{
    internal RotatingPlatformMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
        _requiredEntityActivationsToMove = new(InternalData, 0, x => new(x.GameId));
        _movementPathNodeEulerAngles = new(InternalVectorData, 0, x => new(x));
    }

    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.RotatingPlatform;

    public Branch<AnimIdLeaf> AnimId
    {
        get;
        set
        {
            InternalAnimIdOrItemId = value.GameId;
            field = value;
        }
    }

    private readonly ListRefWrapper<Branch<MapEntityLeaf>, int> _requiredEntityActivationsToMove;
    public IList<Branch<MapEntityLeaf>> RequiredEntityActivationsToMove => _requiredEntityActivationsToMove;

    private readonly ListRefWrapper<Vector3, Vector3> _movementPathNodeEulerAngles;
    public IList<Vector3> MovementPathNodeEulerAngles => _movementPathNodeEulerAngles;

    public float MovementSpeedMultiplier
    {
        get => InternalDialogues[0].Value.y;
        set => InternalDialogues[0].Value.y = value;
    }

    public float FramesDelayBeforeReversingWhenGoingInactiveAtNode
    {
        get => InternalDialogues[1].Value.y;
        set => InternalDialogues[1].Value.y = value;
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

    // TODO: Figure out a way to assign the actual AnimId branch here
    internal override void InitializeFromNew()
    {
        InternalDialogues.AddRange([new(new(0f, 5f, 0f)), new(new(0f, 0f, 0f)), new(new(0f, 0f, 0f))]);
        InternalAnimIdOrItemId = (int)MainManager.AnimIDs.LongAncientPlatform - 1;
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

        _movementPathNodeEulerAngles.SynchronizeFromExistingData(
            InternalVectorData.Select(x => x.Value)
                .ToList());
    }
}