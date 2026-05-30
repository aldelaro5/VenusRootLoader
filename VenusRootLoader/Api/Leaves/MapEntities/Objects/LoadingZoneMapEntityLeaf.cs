using UnityEngine;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects;

public sealed class LoadingZoneMapEntityLeaf : MapEntityLeaf
{
    internal LoadingZoneMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.DoorOtherMap;
    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

    public Vector3 StartingPosition { get => InternalStartingPosition; set => InternalStartingPosition = value; }
    public Vector3 EulerAngles { get => InternalEulerAngles; set => InternalEulerAngles = value; }
    public Branch<MapLeaf> DestinationMap
    {
        get;
        set
        {
            InternalData[0] = value.GameId;
            field = value;
        }
    }

    public Vector3? CameraPositionOffsetFromTargetAfterLoadOverride
    {
        get => InternalData[1] != 1 ? null : InternalVectorData[3];
        set
        {
            if (value is null)
            {
                InternalData[1] = 0;
                InternalVectorData[3] = Vector3.zero;
                return;
            }

            InternalData[1] = 1;
            InternalVectorData[3] = value.Value;
        }
    }

    public Vector3? CameraAnglesOffsetFromTargetAfterLoadOverride
    {
        get => InternalData[2] != 1 ? null : InternalVectorData[4];
        set
        {
            if (value is null)
            {
                InternalData[2] = 0;
                InternalVectorData[4] = Vector3.zero;
                return;
            }

            InternalData[2] = 1;
            InternalVectorData[4] = value.Value;
        }
    }

    public (Vector3 lowerBounds, Vector3 upperBounds)? CameraBoundsAfterLoadOverride
    {
        get => InternalData[3] != 1 ? null : (InternalVectorData[6], InternalVectorData[5]);
        set
        {
            if (value is null)
            {
                InternalData[3] = 0;
                InternalVectorData[6] = Vector3.zero;
                InternalVectorData[5] = Vector3.zero;
                return;
            }

            InternalData[3] = 1;
            InternalVectorData[6] = value.Value.lowerBounds;
            InternalVectorData[5] = value.Value.upperBounds;
        }
    }

    public Vector3? PositionToMoveToBeforeLoad
    {
        get => InternalData[4] == 1 ? null : InternalVectorData[0];
        set
        {
            if (value is null)
            {
                InternalData[4] = 1;
                InternalVectorData[0] = Vector3.zero;
                return;
            }

            InternalData[4] = 0;
            InternalVectorData[0] = value.Value;
        }
    }

    public Vector3 PositionToSpawnAfterMapLoad
    {
        get => InternalVectorData[1];
        set => InternalVectorData[1] = value;
    }

    public Vector3 PositionToMoveFromSpawnAfterMapLoad
    {
        get => InternalVectorData[2];
        set => InternalVectorData[2] = value;
    }

    public float? JumpMovementHeightAfterMapLoad
    {
        get => InternalEmoticonOffset.x;
        set => InternalEmoticonOffset = new(
            value is null or <= 0.1f ? 0f : value.Value,
            InternalEmoticonOffset.y,
            InternalEmoticonOffset.z);
    }

    public Vector3 TriggerBoxColliderSize { get => InternalBoxColSize; set => InternalBoxColSize = value; }
    public Vector3 TriggerBoxColliderCenter { get => InternalBoxColCenter; set => InternalBoxColCenter = value; }

    public int RegionalFlagId { get => InternalRegionalFlagId; set => InternalRegionalFlagId = value; }

    public Branch<FlagLeaf>? ActivationFlag
    {
        get;
        set
        {
            InternalActivationFlagId = value?.GameId ?? -1;
            field = value;
        }
    }

    internal override void InitializeFromNew()
    {
        InternalData.AddRange([-1, 0, 0, 0, 0]);
        InternalVectorData.AddRange(Enumerable.Repeat(Vector3.zero, 7));
        InternalHaxBoxCol = true;
        InternalBoxColIsTrigger = true;
        InternalBoxColSize = Vector3.one;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalData.Count < 5)
            InternalData.AddRange(Enumerable.Repeat(0, 5 - InternalData.Count));
        if (InternalVectorData.Count < 7)
            InternalVectorData.AddRange(Enumerable.Repeat(Vector3.zero, 7 - InternalVectorData.Count));

        ILeavesRegistry<MapLeaf> mapsRegistry = registryResolver.Resolve<MapLeaf>();
        ILeavesRegistry<FlagLeaf> flagsRegistry = registryResolver.Resolve<FlagLeaf>();
        Map = new(mapsRegistry.LeavesByGameIds[InternalData[0]]);

        if (InternalActivationFlagId > 0)
            ActivationFlag = new(flagsRegistry.LeavesByGameIds[InternalActivationFlagId]);
    }
}