using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects;

// TODO: Merge with the multi flags one later with a patch to fix its problems
public sealed class AndBlockOnSingleFlagMapEntityLeaf : MapEntityLeaf
{
    internal AndBlockOnSingleFlagMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.ANDBlock;
    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

    public NegatableFlag FlagInput
    {
        get;
        set
        {
            InternalActivationFlagId = value.EffectiveValue;
            field = value;
        }
    } = null!;

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
        InternalData.AddRange([new(0), new(-1)]);
        InternalVectorData.AddRange(
        [
            new Ref<Vector3>(Vector3.down * 6f),
            new Ref<Vector3>(Vector3.right * 0.1f),
            new Ref<Vector3>(Vector3.zero)
        ]);
        InternalAnimIdOrItemId = (int)MainManager.AnimIDs.PrisonGate - 1;
        InternalActivationFlagId = 0;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalVectorData.Count < 3)
        {
            InternalVectorData.AddRange(
                Enumerable.Repeat(new Ref<Vector3>(Vector3.zero), 3 - InternalVectorData.Count));
        }

        ILeavesRegistry<FlagLeaf> flagsRegistry = registryResolver.Resolve<FlagLeaf>();
        ILeavesRegistry<AnimIdLeaf> animidRegistry = registryResolver.Resolve<AnimIdLeaf>();
        FlagInput = new()
        {
            Flag = new(flagsRegistry.LeavesByGameIds[Math.Abs(InternalActivationFlagId)]),
            IsValueNegated = InternalActivationFlagId < 0
        };

        if (InternalAnimIdOrItemId >= 0)
            AnimId = new(animidRegistry.LeavesByGameIds[InternalAnimIdOrItemId]);
    }
}