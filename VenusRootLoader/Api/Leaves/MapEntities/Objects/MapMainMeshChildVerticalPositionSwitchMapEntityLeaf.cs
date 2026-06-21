using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects;

// TODO: check for wooden switch support
public sealed class MapMainMeshChildVerticalPositionSwitchMapEntityLeaf : ObjectMapEntityLeaf
{
    internal MapMainMeshChildVerticalPositionSwitchMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.WaterSwitch;

    public Branch<AnimIdLeaf> AnimId
    {
        get;
        set
        {
            InternalAnimIdOrItemId = value.GameId;
            field = value;
        }
    }

    public int MapChildIndexToMove { get => InternalData[0].Value; set => InternalData[0].Value = value; }

    public bool CanOnlyBeToggledUsingHornSlashAndHornDash
    {
        get => InternalData[4].Value == 1;
        set => InternalData[4].Value = value ? 1 : 0;
    }

    public float FramesDurationForFullMovement
    {
        get => InternalVectorData[0].Value.x;
        set => InternalVectorData[0].Value.x = value;
    }

    public float VerticalMovementUpperBound
    {
        get => InternalVectorData[0].Value.y;
        set => InternalVectorData[0].Value.y = value;
    }

    public float VerticalMovementLowerBound
    {
        get => InternalVectorData[0].Value.z;
        set => InternalVectorData[0].Value.z = value;
    }

    public Vector3 TriggerBoxColliderSize { get => InternalBoxColSize; set => InternalBoxColSize = value; }
    public Vector3 TriggerBoxColliderCenter { get => InternalBoxColCenter; set => InternalBoxColCenter = value; }

    public Branch<FlagLeaf>? FlagSwitchActivation
    {
        get;
        set
        {
            InternalActivationFlagId = value?.GameId ?? -1;
            field = value;
        }
    }

    [MapEntityInitializeFromNew]
    internal void InitializeFromNew(
        Vector3 startingPosition,
        Branch<AnimIdLeaf> animId,
        int mapChildIndexToMove,
        float verticalMovementLowerBound,
        float verticalMovementUpperBound,
        float framesDurationForFullMovement)
    {
        for (int i = 0; i < 5; i++)
            InternalData.Add(new Ref<int>(0));
        InternalVectorData.Add(new(new(180f, -1f, 1f)));
        InternalHaxBoxCol = true;
        InternalBoxColIsTrigger = true;
        MapChildIndexToMove = mapChildIndexToMove;
        VerticalMovementLowerBound = verticalMovementLowerBound;
        VerticalMovementUpperBound = verticalMovementUpperBound;
        FramesDurationForFullMovement = framesDurationForFullMovement;
        EntityStartingPosition = startingPosition;
        AnimId = animId;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        ILeavesRegistry<FlagLeaf> flagsRegistry = registryResolver.Resolve<FlagLeaf>();

        if (InternalActivationFlagId > 0)
            FlagSwitchActivation = new(flagsRegistry.LeavesByGameIds[InternalActivationFlagId]);
    }
}