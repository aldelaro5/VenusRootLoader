using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.MovingPlatforms;

// TODO: Fix the length 0 issue on NPCControl.CreateEntities so these can move without a switch
public abstract class MovingPlatformMapEntityLeaf : MapEntityLeaf
{
    protected MovingPlatformMapEntityLeaf(int gameId, string namedId, string creatorId) : base(
        gameId,
        namedId,
        creatorId)
    {
        _requiredEntityActivationsToMove = new(InternalData, 0, x => new(x.GameId));
    }

    internal sealed override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal sealed override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.PathPlatform;
    internal sealed override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

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

    private readonly ListRefWrapper<Branch<MapEntityLeaf>, int> _requiredEntityActivationsToMove;
    public IList<Branch<MapEntityLeaf>> RequiredEntityActivationsToMove => _requiredEntityActivationsToMove;

    internal override void InitializeFromNew()
    {
        InternalAnimIdOrItemId = (int)MainManager.AnimIDs.AncientPlatform - 1;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalDialogues.Count < 3)
            InternalDialogues.AddRange(Enumerable.Repeat(new Ref<Vector3>(Vector3.zero), 3 - InternalDialogues.Count));

        ILeavesRegistry<AnimIdLeaf> animIdsRegistry = registryResolver.Resolve<AnimIdLeaf>();
        AnimId = new(animIdsRegistry.LeavesByGameIds[InternalAnimIdOrItemId]);

        _requiredEntityActivationsToMove.SynchronizeFromExistingData(
            InternalData
                .Select(x => new Branch<MapEntityLeaf>(Map.Leaf.EntitiesRegistry.LeavesByGameIds[x.Value]))
                .ToList());
    }
}