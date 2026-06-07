using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.RollingRocks;

public abstract class RollingRockMapEntityLeaf : MapEntityLeaf
{
    protected RollingRockMapEntityLeaf(int gameId, string namedId, string creatorId) : base(gameId, namedId, creatorId)
    {
    }

    internal sealed override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal sealed override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.RollingRock;
    internal sealed override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

    public Vector3 StartingPosition { get => InternalStartingPosition; set => InternalStartingPosition = value; }

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
        InternalAnimIdOrItemId = (int)MainManager.AnimIDs.RollingRock - 1;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalData.Count < 4)
            InternalData.AddRange(Enumerable.Repeat(new Ref<int>(-1), 4 - InternalData.Count));
    }
}