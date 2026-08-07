using UnityEngine;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects;

public sealed class ResetCameraZoneMapEntityLeaf : ObjectMapEntityLeaf
{
    internal ResetCameraZoneMapEntityLeaf(int gameId, string creatorId, string namedId)
        : base(gameId, creatorId, namedId)
    {
    }

    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.ResetCamera;

    public Vector3 TriggerBoxColliderSize { get => InternalBoxColSize; set => InternalBoxColSize = value; }
    public Vector3 TriggerBoxColliderCenter { get => InternalBoxColCenter; set => InternalBoxColCenter = value; }

    [MapEntityInitializeFromNew]
    internal void InitializeFromNew(
        Vector3 startingPosition,
        Vector3 triggerBoxColliderSize,
        Vector3 triggerBoxColliderCenter)
    {
        InternalHaxBoxCol = true;
        InternalBoxColIsTrigger = true;
        TriggerBoxColliderSize = triggerBoxColliderSize;
        TriggerBoxColliderCenter = triggerBoxColliderCenter;
        EntityStartingPosition = startingPosition;
    }

    internal override void InitializeFromExisting() { }
}