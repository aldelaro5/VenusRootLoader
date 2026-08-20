using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.MovingPlatforms;

public abstract class MovingPlatformMapEntityLeaf : ObjectMapEntityLeaf
{
    protected MovingPlatformMapEntityLeaf(int gameId, string creatorId, string namedId) : base(
        gameId,
        creatorId,
        namedId)
    {
        _requiredEntityActivationsToMove = new(InternalData, 0, x => new(x.Resolve().GameId));
    }

    internal sealed override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.PathPlatform;

    public Branch<AnimIdLeaf> AnimId
    {
        get;
        set
        {
            InternalAnimIdOrItemId = value.Resolve().GameId;
            field = value;
        }
    } = null!;

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
        InternalStartingPosition = startingPosition;
        foreach (Branch<ObjectMapEntityLeaf> requiredEntityActivation in requiredEntityActivationsToMove)
            RequiredEntityActivationsToMove.Add(requiredEntityActivation);
    }

    internal override void InitializeFromExisting()
    {
        if (InternalDialogues.Count < 3)
        {
            int count = InternalDialogues.Count;
            for (int i = 0; i < 3 - count; i++)
                InternalDialogues.Add(new Ref<Vector3>(Vector3.zero));
        }

        ILeavesRegistry<AnimIdLeaf> animIdsRegistry = RegistryResolver.Resolve<AnimIdLeaf>();
        AnimId = new(animIdsRegistry.GetByGameId(InternalAnimIdOrItemId));

        _requiredEntityActivationsToMove.SynchronizeFromExistingData(
            InternalData
                .Select(x =>
                    new Branch<ObjectMapEntityLeaf>(
                        (ObjectMapEntityLeaf)Map.Resolve().EntitiesRegistry.GetByGameId(x.Value)))
                .ToList());
    }
}