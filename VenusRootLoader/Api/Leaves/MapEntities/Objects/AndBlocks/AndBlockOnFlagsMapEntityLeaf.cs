using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.AndBlocks;

public sealed class AndBlockOnFlagsMapEntityLeaf : MapEntityLeaf
{
    internal AndBlockOnFlagsMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
        _flagsInput = new(InternalData, 1, x => new(x.GameId));
    }

    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.ANDBlock;
    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

    private readonly ListRefWrapper<Branch<FlagLeaf>, int> _flagsInput;
    public IList<Branch<FlagLeaf>> FlagsInput => _flagsInput;

    public Vector3 StartingPosition { get => InternalStartingPosition; set => InternalStartingPosition = value; }
    public Vector3 EulerAngles { get => InternalEulerAngles; set => InternalEulerAngles = value; }

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
        InternalData.AddRange([new Ref<int>(-2)]);
        InternalVectorData.AddRange(
        [
            new Ref<Vector3>(Vector3.down * 6f),
            new Ref<Vector3>(Vector3.right * 0.1f),
            new Ref<Vector3>(Vector3.zero)
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

        ILeavesRegistry<FlagLeaf> flagsRegistry = registryResolver.Resolve<FlagLeaf>();
        ILeavesRegistry<AnimIdLeaf> animIdRegistry = registryResolver.Resolve<AnimIdLeaf>();

        _flagsInput.SynchronizeFromExistingData(
            InternalData
                .Skip(1)
                .Select(x => new Branch<FlagLeaf>(flagsRegistry.LeavesByGameIds[Math.Abs(x.Value)]))
                .ToList());

        if (InternalAnimIdOrItemId >= 0)
            AnimId = new(animIdRegistry.LeavesByGameIds[InternalAnimIdOrItemId]);
    }
}