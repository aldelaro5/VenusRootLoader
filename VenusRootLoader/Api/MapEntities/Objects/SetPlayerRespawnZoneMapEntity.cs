using UnityEngine;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.MapEntities.Objects;

public sealed class SetPlayerRespawnZoneMapEntity : MapEntity
{
    internal SetPlayerRespawnZoneMapEntity(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.SetPlayerRespawn;
    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

    public Vector3 StartingPosition { get => InternalStartingPosition; set => InternalStartingPosition = value; }
    public Vector3 EulerAngles { get => InternalEulerAngles; set => InternalEulerAngles = value; }

    public Vector3? RespawnPositionToSetWhenTriggered
    {
        get => InternalVectorData[0].magnitude < 0.1 ? null : InternalVectorData[0];
        set => InternalVectorData[0] = value is null or { magnitude: < 0.1f } ? Vector3.zero : value.Value;
    }

    public Vector3 TriggerBoxColliderSize { get => InternalBoxColSize; set => InternalBoxColSize = value; }
    public Vector3 TriggerBoxColliderCenter { get => InternalBoxColCenter; set => InternalBoxColCenter = value; }

    internal override void InitializeFromNew()
    {
        InternalVectorData.Add(Vector3.back * 0.2f);
        InternalHaxBoxCol = true;
        InternalBoxColIsTrigger = true;
        InternalBoxColSize = Vector3.one;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalVectorData.Count == 0)
            InternalVectorData.Add(Vector3.zero);
    }
}