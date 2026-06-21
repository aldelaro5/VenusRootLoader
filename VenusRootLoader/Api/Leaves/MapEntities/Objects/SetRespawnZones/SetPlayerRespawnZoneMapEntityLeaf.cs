using CommunityToolkit.Diagnostics;
using UnityEngine;
using VenusRootLoader.Registry;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.SetRespawnZones;

public sealed class SetPlayerRespawnZoneMapEntityLeaf : ObjectMapEntityLeaf
{
    internal SetPlayerRespawnZoneMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.SetPlayerRespawn;

    public Vector3 RespawnPositionToSetWhenTriggered
    {
        get => InternalVectorData[0].Value;
        set
        {
            Guard.IsGreaterThanOrEqualTo(value.magnitude, 0.1f);
            InternalVectorData[0].Value = value;
        }
    }

    public Vector3 TriggerBoxColliderSize { get => InternalBoxColSize; set => InternalBoxColSize = value; }
    public Vector3 TriggerBoxColliderCenter { get => InternalBoxColCenter; set => InternalBoxColCenter = value; }

    [MapEntityInitializeFromNew]
    internal void InitializeFromNew(
        Vector3 startingPosition,
        Vector3 respawnPositionToSetWhenTriggered,
        Vector3 triggerBoxColliderSize,
        Vector3 triggerBoxColliderCenter)
    {
        Guard.IsGreaterThanOrEqualTo(respawnPositionToSetWhenTriggered.magnitude, 0.1f);
        EntityStartingPosition = startingPosition;
        InternalHaxBoxCol = true;
        InternalBoxColIsTrigger = true;
        TriggerBoxColliderSize = triggerBoxColliderSize;
        TriggerBoxColliderCenter = triggerBoxColliderCenter;
        InternalVectorData.Add(new(respawnPositionToSetWhenTriggered));
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
    }
}