using UnityEngine;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects;

public sealed class ResetCameraZoneMapEntityLeaf : ObjectMapEntityLeaf
{
    internal ResetCameraZoneMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.ResetCamera;

    public Vector3 TriggerBoxColliderSize { get => InternalBoxColSize; set => InternalBoxColSize = value; }
    public Vector3 TriggerBoxColliderCenter { get => InternalBoxColCenter; set => InternalBoxColCenter = value; }

    internal override void InitializeFromNew()
    {
        InternalHaxBoxCol = true;
        InternalBoxColIsTrigger = true;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver) { }
}