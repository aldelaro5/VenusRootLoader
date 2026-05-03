using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.MapEntities;

public sealed class InsideTransitionZoneMapEntity : MapEntity
{
    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.DoorSameMap;

    public Vector3 StartingPosition { get => InternalStartingPosition; set => InternalStartingPosition = value; }

    public int InsideIdUsedForTransition { get => InternalData[0]; set => InternalData[0] = value; }

    public Branch<MusicLeaf>? MusicUsedWhileInside
    {
        get;
        set
        {
            InternalData[1] = value?.GameId ?? -1;
            field = value;
        }
    }

    public Vector3 PositionToMoveToWhenEntering { get => InternalVectorData[0]; set => InternalVectorData[0] = value; }
    public Vector3 PositionToMoveToWhenExiting { get => InternalVectorData[1]; set => InternalVectorData[1] = value; }

    public Vector3? CameraPositionOffsetFromTargetWhileInside
    {
        get => InternalVectorData[2].magnitude < 0.1f ? null : InternalVectorData[2];
        set => InternalVectorData[2] = value is null || value.Value.magnitude < 0.1f ? Vector3.zero : value.Value;
    }

    public Vector3? CameraAnglesOffsetFromTargetWhileInside
    {
        get => InternalVectorData[3].magnitude < 0.1f ? null : InternalVectorData[3];
        set => InternalVectorData[3] = value is null || value.Value.magnitude < 0.1f ? Vector3.zero : value.Value;
    }

    public Vector3? CameraLowerBoundsWhileInside
    {
        get => InternalVectorData[7].magnitude < 0.1f ? null : InternalVectorData[7];
        set => InternalVectorData[7] = value is null || value.Value.magnitude < 0.1f ? Vector3.zero : value.Value;
    }

    public Vector3? CameraUpperBoundsWhileInside
    {
        get => InternalVectorData[6].magnitude < 0.1f ? null : InternalVectorData[6];
        set => InternalVectorData[6] = value is null || value.Value.magnitude < 0.1f ? Vector3.zero : value.Value;
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

    internal InsideTransitionZoneMapEntity() { }

    internal override void InitializeFromNew()
    {
        InternalData.AddRange([-1, -1]);
        InternalVectorData.AddRange(Enumerable.Repeat(Vector3.zero, 8));
        InternalHaxBoxCol = true;
        InternalBoxColIsTrigger = true;
        InternalBoxColSize = Vector3.one;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalData.Count < 2)
            InternalData.AddRange(Enumerable.Repeat(-1, 2 - InternalData.Count));
        if (InternalVectorData.Count < 8)
            InternalVectorData.AddRange(Enumerable.Repeat(Vector3.zero, 8 - InternalVectorData.Count));

        ILeavesRegistry<MusicLeaf> musicRegistry = registryResolver.Resolve<MusicLeaf>();
        ILeavesRegistry<FlagLeaf> flagsRegistry = registryResolver.Resolve<FlagLeaf>();
        MusicUsedWhileInside = InternalData[1] == -1 ? null : new(musicRegistry.LeavesByGameIds[InternalData[1]]);

        if (InternalActivationFlagId > 0)
            ActivationFlag = new(flagsRegistry.LeavesByGameIds[InternalActivationFlagId]);
    }
}