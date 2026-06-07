using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.AndBlocks;

public abstract class AndBlockMapEntityLeaf : MapEntityLeaf
{
    protected AndBlockMapEntityLeaf(int gameId, string namedId, string creatorId) : base(gameId, namedId, creatorId)
    {
    }

    internal sealed override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal sealed override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.ANDBlock;
    internal sealed override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

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

    public Vector3? EntityStartScale
    {
        get => InternalVectorData[2].Value.magnitude <= 0.1f ? null : InternalVectorData[2].Value;
        set => InternalVectorData[2].Value = value ?? Vector3.zero;
    }

    internal override void InitializeFromNew()
    {
        InternalVectorData.AddRange(
        [
            new(Vector3.down * 6f),
            new(Vector3.right * 0.1f),
            new(Vector3.zero)
        ]);
        InternalAnimIdOrItemId = (int)MainManager.AnimIDs.PrisonGate - 1;
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