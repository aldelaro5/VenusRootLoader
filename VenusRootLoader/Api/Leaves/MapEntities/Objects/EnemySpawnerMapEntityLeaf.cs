using CommunityToolkit.Diagnostics;
using UnityEngine;
using VenusRootLoader.Api.Leaves.MapEntities.Enemies;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects;

public sealed class EnemySpawnerMapEntityLeaf : ObjectMapEntityLeaf
{
    internal EnemySpawnerMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.EnemySpawner;

    public Branch<EnemyEncounterMapEntityLeaf> EnemyToRespawn
    {
        get;
        set
        {
            if (value.Leaf.Map != Map)
                ThrowHelper.ThrowInvalidOperationException($"This map enemy must be in the {Map.NamedId} map");

            InternalData[0].Value = value.GameId;
            field = value;
        }
    }

    public int FramesDelayBeforeRespawn { get => InternalData[4].Value; set => InternalData[4].Value = value; }
    public Vector3 RespawnCenter { get => InternalVectorData[0].Value; set => InternalVectorData[0].Value = value; }

    public Vector3 RespawnRadiusRangeFromPosition
    {
        get => InternalVectorData[1].Value;
        set => InternalVectorData[1].Value = value;
    }

    internal void InitializeFromNew(
        Vector3 startingPosition,
        Branch<EnemyEncounterMapEntityLeaf> enemyToRespawn,
        Vector3 respawnCenter,
        Vector3 respawnRadiusRangeFromPosition)
    {
        InternalData.AddRange([new(0), new(0), new(0), new(0), new(300), new(-1)]);
        InternalVectorData.AddRange([new(Vector3.zero), new(Vector3.one)]);
        EnemyToRespawn = enemyToRespawn;
        RespawnCenter = respawnCenter;
        RespawnRadiusRangeFromPosition = respawnRadiusRangeFromPosition;
        EntityStartingPosition = startingPosition;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        EnemyToRespawn =
            (Branch<EnemyEncounterMapEntityLeaf>)Map.Leaf.EntitiesRegistry.LeavesByGameIds[InternalData[0].Value]!;
    }
}