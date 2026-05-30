using UnityEngine;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects;

public sealed class EnemySpawnerMapEntityLeaf : MapEntityLeaf
{
    internal EnemySpawnerMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.EnemySpawner;
    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

    public Vector3 StartingPosition { get => InternalStartingPosition; set => InternalStartingPosition = value; }
    public Vector3 EulerAngles { get => InternalEulerAngles; set => InternalEulerAngles = value; }

    public int MapEntityIdEnemyToRespawn { get => InternalData[0]; set => InternalData[0] = value; }
    public int FramesBeforeRespawn { get => InternalData[4]; set => InternalData[4] = value; }
    public Vector3 RespawnCenter { get => InternalVectorData[0]; set => InternalVectorData[0] = value; }
    public Vector3 RespawnRadiusRange { get => InternalVectorData[1]; set => InternalVectorData[1] = value; }

    internal override void InitializeFromNew()
    {
        InternalData.AddRange([0, 0, 0, 0, 300, -1]);
        InternalVectorData.AddRange([Vector3.zero, Vector3.one]);
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver) { }
}