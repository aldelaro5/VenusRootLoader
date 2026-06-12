using UnityEngine;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects;

public sealed class SetPlayerRespawnZoneMapEntityLeaf : ObjectMapEntityLeaf
{
    internal SetPlayerRespawnZoneMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.SetPlayerRespawn;

    public Vector3? RespawnPositionToSetWhenTriggered
    {
        get => InternalVectorData[0].Value.magnitude < 0.1 ? null : InternalVectorData[0].Value;
        set => InternalVectorData[0].Value = value is null or { magnitude: < 0.1f } ? Vector3.zero : value.Value;
    }

    public Vector3 TriggerBoxColliderSize { get => InternalBoxColSize; set => InternalBoxColSize = value; }
    public Vector3 TriggerBoxColliderCenter { get => InternalBoxColCenter; set => InternalBoxColCenter = value; }

    internal void InitializeFromNew(
        Vector3 startingPosition,
        Vector3? respawnPositionToSetWhenTriggered,
        Vector3 triggerBoxColliderSize,
        Vector3 triggerBoxColliderCenter)
    {
        InternalVectorData.Add(new(Vector3.back * 0.2f));
        InternalHaxBoxCol = true;
        InternalBoxColIsTrigger = true;
        RespawnPositionToSetWhenTriggered = respawnPositionToSetWhenTriggered;
        TriggerBoxColliderSize = triggerBoxColliderSize;
        TriggerBoxColliderCenter = triggerBoxColliderCenter;
        EntityStartingPosition = startingPosition;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalVectorData.Count == 0)
            InternalVectorData.Add(new(Vector3.zero));
    }
}