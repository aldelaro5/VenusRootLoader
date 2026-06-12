using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects;

public sealed class LoadingZoneMapEntityLeaf : ObjectMapEntityLeaf
{
    internal LoadingZoneMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.DoorOtherMap;

    public Branch<MapLeaf> DestinationMap
    {
        get;
        set
        {
            InternalData[0].Value = value.GameId;
            field = value;
        }
    }

    public Vector3? CameraPositionOffsetFromTargetAfterLoadOverride
    {
        get => InternalData[1].Value != 1 ? null : InternalVectorData[3].Value;
        set
        {
            if (value is null)
            {
                InternalData[1].Value = 0;
                InternalVectorData[3].Value = Vector3.zero;
                return;
            }

            InternalData[1].Value = 1;
            InternalVectorData[3].Value = value.Value;
        }
    }

    public Vector3? CameraAnglesOffsetFromTargetAfterLoadOverride
    {
        get => InternalData[2].Value != 1 ? null : InternalVectorData[4].Value;
        set
        {
            if (value is null)
            {
                InternalData[2].Value = 0;
                InternalVectorData[4].Value = Vector3.zero;
                return;
            }

            InternalData[2].Value = 1;
            InternalVectorData[4].Value = value.Value;
        }
    }

    public (Vector3 lowerBounds, Vector3 upperBounds)? CameraBoundsAfterLoadOverride
    {
        get => InternalData[3].Value != 1 ? null : (InternalVectorData[6].Value, InternalVectorData[5].Value);
        set
        {
            if (value is null)
            {
                InternalData[3].Value = 0;
                InternalVectorData[6].Value = Vector3.zero;
                InternalVectorData[5].Value = Vector3.zero;
                return;
            }

            InternalData[3].Value = 1;
            InternalVectorData[6].Value = value.Value.lowerBounds;
            InternalVectorData[5].Value = value.Value.upperBounds;
        }
    }

    public Vector3? PositionToMoveToBeforeLoad
    {
        get => InternalData[4].Value == 1 ? null : InternalVectorData[0].Value;
        set
        {
            if (value is null)
            {
                InternalData[4].Value = 1;
                InternalVectorData[0].Value = Vector3.zero;
                return;
            }

            InternalData[4].Value = 0;
            InternalVectorData[0].Value = value.Value;
        }
    }

    public Vector3 PositionToSpawnAfterLoad
    {
        get => InternalVectorData[1].Value;
        set => InternalVectorData[1].Value = value;
    }

    public Vector3 PositionToMoveFromSpawnAfterLoad
    {
        get => InternalVectorData[2].Value;
        set => InternalVectorData[2].Value = value;
    }

    public float? JumpMovementHeightAfterLoad
    {
        get => InternalEmoticonOffset.x;
        set => InternalEmoticonOffset = new(
            value is null or <= 0.1f ? 0f : value.Value,
            InternalEmoticonOffset.y,
            InternalEmoticonOffset.z);
    }

    public Vector3 TriggerBoxColliderSize { get => InternalBoxColSize; set => InternalBoxColSize = value; }
    public Vector3 TriggerBoxColliderCenter { get => InternalBoxColCenter; set => InternalBoxColCenter = value; }

    public Branch<FlagLeaf>? ActivationFlag
    {
        get;
        set
        {
            InternalActivationFlagId = value?.GameId ?? -1;
            field = value;
        }
    }

    internal void InitializeFromNew(
        Vector3 startingPosition,
        Branch<MapLeaf> destinationMap,
        Vector3 positionToSpawnAfterLoad,
        Vector3 positionToMoveFromSpawnAfterLoad,
        Vector3 triggerBoxColliderSize,
        Vector3 triggerBoxColliderCenter)
    {
        InternalData.AddRange([new(-1), new(0), new(0), new(0), new(0)]);
        InternalVectorData.AddRange(Enumerable.Repeat(new Ref<Vector3>(Vector3.zero), 7));
        InternalHaxBoxCol = true;
        InternalBoxColIsTrigger = true;
        InternalBoxColSize = Vector3.one;
        DestinationMap = destinationMap;
        PositionToSpawnAfterLoad = positionToSpawnAfterLoad;
        PositionToMoveFromSpawnAfterLoad = positionToMoveFromSpawnAfterLoad;
        TriggerBoxColliderSize = triggerBoxColliderSize;
        TriggerBoxColliderCenter = triggerBoxColliderCenter;
        EntityStartingPosition = startingPosition;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalData.Count < 5)
            InternalData.AddRange(Enumerable.Repeat(new Ref<int>(0), 5 - InternalData.Count));
        if (InternalVectorData.Count < 7)
        {
            InternalVectorData.AddRange(
                Enumerable.Repeat(new Ref<Vector3>(Vector3.zero), 7 - InternalVectorData.Count));
        }

        ILeavesRegistry<MapLeaf> mapsRegistry = registryResolver.Resolve<MapLeaf>();
        ILeavesRegistry<FlagLeaf> flagsRegistry = registryResolver.Resolve<FlagLeaf>();
        Map = new(mapsRegistry.LeavesByGameIds[InternalData[0].Value]);

        if (InternalActivationFlagId > 0)
            ActivationFlag = new(flagsRegistry.LeavesByGameIds[InternalActivationFlagId]);
    }
}