using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.AndBlocks;

// TODO: Defaults animId to PrisonGate in venus
public abstract class AndBlockMapEntityLeaf : ObjectMapEntityLeaf
{
    protected AndBlockMapEntityLeaf(int gameId, string namedId, string creatorId) : base(gameId, namedId, creatorId)
    {
    }

    internal sealed override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.ANDBlock;

    public Branch<AnimIdLeaf>? AnimId
    {
        get;
        set
        {
            InternalAnimIdOrItemId = value?.GameId ?? -1;
            field = value;
        }
    }

    public Vector3 LocalPositionWhenActuatedAfterLerp
    {
        get => InternalVectorData[0].Value;
        set => InternalVectorData[0].Value = value;
    }

    public float LocalPositionLerpFactorWhenActuated
    {
        get => InternalVectorData[1].Value.x;
        set => InternalVectorData[1].Value.x = value;
    }

    public Vector3? EntityStartScaleOverride
    {
        get => InternalVectorData[2].Value.magnitude <= 0.1f ? null : InternalVectorData[2].Value;
        set => InternalVectorData[2].Value = value ?? Vector3.zero;
    }

    protected void InitializeFromNew(Vector3 startingPosition, Branch<AnimIdLeaf>? animId)
    {
        EntityStartingPosition = startingPosition;
        InternalVectorData.AddRange(
        [
            new(Vector3.down * 6f),
            new(Vector3.right * 0.1f),
            new(Vector3.zero)
        ]);
        AnimId = animId;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalVectorData.Count < 3)
        {
            InternalVectorData.AddRange(
                Enumerable.Repeat(new Ref<Vector3>(Vector3.zero), 3 - InternalVectorData.Count));
        }

        ILeavesRegistry<AnimIdLeaf> animIdRegistry = registryResolver.Resolve<AnimIdLeaf>();

        if (InternalAnimIdOrItemId >= 0)
            AnimId = new(animIdRegistry.LeavesByGameIds[InternalAnimIdOrItemId]);
    }
}