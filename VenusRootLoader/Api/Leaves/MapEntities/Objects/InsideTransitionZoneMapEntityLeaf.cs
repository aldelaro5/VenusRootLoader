using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects;

public sealed class InsideTransitionZoneMapEntityLeaf : ObjectMapEntityLeaf
{
    internal InsideTransitionZoneMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.DoorSameMap;

    public int InsideIdUsedForTransition { get => InternalData[0].Value; set => InternalData[0].Value = value; }

    public Branch<MusicLeaf>? MusicOverrideWhileInside
    {
        get;
        set
        {
            InternalData[1].Value = value?.GameId ?? -1;
            field = value;
        }
    }

    public Vector3 PositionToMoveToWhenEntering
    {
        get => InternalVectorData[0].Value;
        set => InternalVectorData[0].Value = value;
    }

    public Vector3 PositionToMoveToWhenExiting
    {
        get => InternalVectorData[1].Value;
        set => InternalVectorData[1].Value = value;
    }

    public Vector3? CameraPositionOffsetFromTargetOverrideWhileInside
    {
        get => InternalVectorData[2].Value.magnitude < 0.1f ? null : InternalVectorData[2].Value;
        set => InternalVectorData[2].Value = value is null || value.Value.magnitude < 0.1f ? Vector3.zero : value.Value;
    }

    public Vector3? CameraAnglesOffsetFromTargetOverrideWhileInside
    {
        get => InternalVectorData[3].Value.magnitude < 0.1f ? null : InternalVectorData[3].Value;
        set => InternalVectorData[3].Value = value is null || value.Value.magnitude < 0.1f ? Vector3.zero : value.Value;
    }

    public Vector3? CameraLowerBoundsOverrideWhileInside
    {
        get => InternalVectorData[7].Value.magnitude < 0.1f ? null : InternalVectorData[7].Value;
        set => InternalVectorData[7].Value = value is null || value.Value.magnitude < 0.1f ? Vector3.zero : value.Value;
    }

    public Vector3? CameraUpperBoundsOverrideWhileInside
    {
        get => InternalVectorData[6].Value.magnitude < 0.1f ? null : InternalVectorData[6].Value;
        set => InternalVectorData[6].Value = value is null || value.Value.magnitude < 0.1f ? Vector3.zero : value.Value;
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

    internal override void InitializeFromNew()
    {
        InternalData.AddRange([new(-1), new(-1)]);
        InternalVectorData.AddRange(Enumerable.Repeat(new Ref<Vector3>(Vector3.zero), 8));
        InternalHaxBoxCol = true;
        InternalBoxColIsTrigger = true;
        InternalBoxColSize = Vector3.one;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalData.Count < 2)
            InternalData.AddRange(Enumerable.Repeat(new Ref<int>(-1), 2 - InternalData.Count));
        if (InternalVectorData.Count < 8)
            InternalVectorData.AddRange(
                Enumerable.Repeat(new Ref<Vector3>(Vector3.zero), 8 - InternalVectorData.Count));

        ILeavesRegistry<MusicLeaf> musicRegistry = registryResolver.Resolve<MusicLeaf>();
        ILeavesRegistry<FlagLeaf> flagsRegistry = registryResolver.Resolve<FlagLeaf>();
        MusicOverrideWhileInside = InternalData[1].Value == -1
            ? null
            : new(musicRegistry.LeavesByGameIds[InternalData[1].Value]);

        if (InternalActivationFlagId > 0)
            ActivationFlag = new(flagsRegistry.LeavesByGameIds[InternalActivationFlagId]);
    }
}