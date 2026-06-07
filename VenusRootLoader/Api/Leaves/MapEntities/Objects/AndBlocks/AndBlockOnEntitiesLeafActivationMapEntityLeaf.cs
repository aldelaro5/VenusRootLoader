using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.AndBlocks;

public sealed class AndBlockOnEntitiesLeafActivationMapEntityLeaf : MapEntityLeaf
{
    internal AndBlockOnEntitiesLeafActivationMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
        _entityActivationsInput = new(InternalData, 1, x => x.Ref);
    }

    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.ANDBlock;
    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

    private readonly ListRefWrapper<NegatableMapEntityActivation, int> _entityActivationsInput;
    public IList<NegatableMapEntityActivation> EntityActivationsInput => _entityActivationsInput;

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
        InternalData.AddRange([new Ref<int>(-1)]);
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
            InternalVectorData.AddRange(
                Enumerable.Repeat(new Ref<Vector3>(Vector3.zero), 3 - InternalVectorData.Count));

        MapLeaf map = registryResolver.Resolve<MapLeaf>().LeavesByGameIds[Map.GameId];
        ILeavesRegistry<AnimIdLeaf> animidRegistry = registryResolver.Resolve<AnimIdLeaf>();

        _entityActivationsInput.SynchronizeFromExistingData(
            InternalData
                .Skip(1)
                .Select(x => new NegatableMapEntityActivation
                {
                    MapEntity = map.EntitiesRegistry.LeavesByGameIds[Math.Abs(x.Value)],
                    IsActivationValueNegated = x.Value < 0
                })
                .ToList());

        if (InternalAnimIdOrItemId >= 0)
            AnimId = new(animidRegistry.LeavesByGameIds[InternalAnimIdOrItemId]);
    }
}