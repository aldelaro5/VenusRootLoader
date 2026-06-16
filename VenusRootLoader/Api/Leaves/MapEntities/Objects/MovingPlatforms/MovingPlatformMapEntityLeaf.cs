using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.MovingPlatforms;

// TODO: Fix the length 0 issue on NPCControl.CreateEntities so these can move without a switch
public abstract class MovingPlatformMapEntityLeaf : ObjectMapEntityLeaf
{
    protected MovingPlatformMapEntityLeaf(int gameId, string namedId, string creatorId) : base(
        gameId,
        namedId,
        creatorId)
    {
        _requiredEntityActivationsToMove = new(InternalData, 0, x => new(x.GameId));
    }

    internal sealed override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.PathPlatform;

    public Branch<AnimIdLeaf> AnimId
    {
        get;
        set
        {
            InternalAnimIdOrItemId = value.GameId;
            field = value;
        }
    }

    public float MovementSpeedMultiplier
    {
        get => InternalDialogues[0].Value.y;
        set => InternalDialogues[0].Value.y = value;
    }

    public float? ModelScaleOverride
    {
        get => InternalDialogues[2].Value.x <= 0.1f ? null : InternalDialogues[2].Value.x / 10f;
        set => InternalDialogues[2].Value.x = value is > 0.1f ? value.Value * 10f : 0f;
    }

    public float? FramesBeforeShockIfElectroPlatformOverride
    {
        get => InternalDialogues[2].Value.y == 0f ? null : InternalDialogues[2].Value.y;
        set => InternalDialogues[2].Value.y = value ?? 0f;
    }

    private readonly ListRefWrapper<Branch<ObjectMapEntityLeaf>, int> _requiredEntityActivationsToMove;
    public IList<Branch<ObjectMapEntityLeaf>> RequiredEntityActivationsToMove => _requiredEntityActivationsToMove;

    protected void InitializeFromNew(
        Vector3 startingPosition,
        Branch<AnimIdLeaf> animId,
        IList<Branch<ObjectMapEntityLeaf>> requiredEntityActivationsToMove)
    {
        AnimId = animId;
        EntityStartingPosition = startingPosition;
        foreach (Branch<ObjectMapEntityLeaf> requiredEntityActivation in requiredEntityActivationsToMove)
            RequiredEntityActivationsToMove.Add(requiredEntityActivation);
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalDialogues.Count < 3)
            InternalDialogues.AddRange(Enumerable.Repeat(new Ref<Vector3>(Vector3.zero), 3 - InternalDialogues.Count));

        ILeavesRegistry<AnimIdLeaf> animIdsRegistry = registryResolver.Resolve<AnimIdLeaf>();
        AnimId = new(animIdsRegistry.LeavesByGameIds[InternalAnimIdOrItemId]);

        _requiredEntityActivationsToMove.SynchronizeFromExistingData(
            InternalData
                .Select(x =>
                    new Branch<ObjectMapEntityLeaf>(
                        (ObjectMapEntityLeaf)Map.Leaf.EntitiesRegistry.LeavesByGameIds[x.Value]))
                .ToList());
    }
}