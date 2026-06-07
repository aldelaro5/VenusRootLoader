using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.RollingRocks;

public enum RollingRockScheme
{
    RollOnSpawnWithoutImpactEffect = 0,
    RollWhenHittingGroundWithImpactEffect = 1
}

public sealed class RollingRockMapEntityLeaf : MapEntityLeaf
{
    internal RollingRockMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.RollingRock;
    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

    public Vector3 StartingPosition { get => InternalStartingPosition; set => InternalStartingPosition = value; }

    public RollingRockScheme RollingScheme
    {
        get => (RollingRockScheme)InternalData[0].Value;
        set => InternalData[0].Value = (int)value;
    }

    public Vector3 DestinationPosition
    {
        get => InternalVectorData[0].Value;
        set => InternalVectorData[0].Value = value;
    }

    public float MinimumYPositionBeforeRespawn
    {
        get => InternalVectorData[1].Value.x;
        set => InternalVectorData[1].Value.x = value;
    }

    public float? RockRadiusOverride
    {
        get => InternalVectorData[1].Value.y < 0.1 ? null : InternalVectorData[1].Value.y;
        set => InternalVectorData[1].Value.y = value is null or < 0.1f ? 0f : value.Value;
    }

    public Vector3 RollingRotationAngles
    {
        get => InternalVectorData[2].Value;
        set => InternalVectorData[2].Value = value;
    }

    internal override void InitializeFromNew()
    {
        InternalData.AddRange([new(1), new(0), new(0), new(-1)]);
        InternalVectorData.AddRange([new(new(10f, 0f, 0f)), new(new(-10f, 0f, 0f)), new(new(0f, 0f, 5f))]);
        InternalAnimIdOrItemId = (int)MainManager.AnimIDs.RollingRock - 1;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalData.Count < 4)
            InternalData.AddRange(Enumerable.Repeat(new Ref<int>(-1), 4 - InternalData.Count));
    }
}