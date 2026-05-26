using UnityEngine;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.MapEntities.Objects;

public enum RollingRockScheme
{
    RollOnSpawnWithoutImpactEffect = 0,
    RollWhenHittingGroundWithImpactEffect = 1
}

public sealed class RollingRockMapEntity : MapEntity
{
    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.RollingRock;
    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

    public Vector3 StartingPosition { get => InternalStartingPosition; set => InternalStartingPosition = value; }

    public RollingRockScheme RollingScheme
    {
        get => (RollingRockScheme)InternalData[0];
        set => InternalData[0] = (int)value;
    }

    public Vector3 DestinationPosition { get => InternalVectorData[0]; set => InternalVectorData[0] = value; }

    public float MinimumYPositionBeforeRespawn
    {
        get => InternalVectorData[1].x;
        set => InternalVectorData[1] = new(value, InternalVectorData[1].y, InternalVectorData[1].z);
    }

    public float? RockRadiusOverride
    {
        get => InternalVectorData[1].y < 0.1 ? null : InternalVectorData[1].y;
        set => InternalVectorData[1] = new(
            InternalVectorData[1].x,
            value is null or < 0.1f ? 0f : value.Value,
            InternalVectorData[1].z);
    }

    public Vector3 RollingRotationAngles { get => InternalVectorData[2]; set => InternalVectorData[2] = value; }

    internal RollingRockMapEntity() { }

    internal override void InitializeFromNew()
    {
        InternalData.AddRange([1, 0, 0, -1]);
        InternalVectorData.AddRange([new(10f, 0f, 0f), new(-10f, 0f, 0f), new(0f, 0f, 5f)]);
        InternalAnimIdOrItemId = (int)MainManager.AnimIDs.RollingRock - 1;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalData.Count < 4)
            InternalData.AddRange(Enumerable.Repeat(-1, 4 - InternalData.Count));
    }
}