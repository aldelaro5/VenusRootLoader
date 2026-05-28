using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.MapEntities.Objects;

public sealed class MusicChangeZoneMapEntity : MapEntity
{
    internal MusicChangeZoneMapEntity(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.MusicRange;
    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

    public Vector3 StartingPosition { get => InternalStartingPosition; set => InternalStartingPosition = value; }

    public int FramesDelayBeforeMusicChange { get => InternalData[1]; set => InternalData[1] = value; }

    public Branch<MusicLeaf> MusicWhenInRange
    {
        get;
        set
        {
            InternalData[2] = value.GameId;
            field = value;
        }
    }

    public float RangeRadiusFromPosition
    {
        get => InternalVectorData[0].x;
        set => InternalVectorData[0] = new(value, InternalVectorData[0].y, InternalVectorData[0].z);
    }

    public float MusicFadeRate
    {
        get => InternalVectorData[0].y;
        set => InternalVectorData[0] = new(InternalVectorData[0].x, value, InternalVectorData[0].z);
    }

    public float MusicMaxVolumeMultiplier
    {
        get => InternalVectorData[0].z;
        set => InternalVectorData[0] = new(InternalVectorData[0].x, InternalVectorData[0].y, value);
    }

    internal override void InitializeFromNew()
    {
        InternalData.AddRange([0, 20, 0]);
        InternalVectorData.AddRange([new(10f, 0.1f, 1f), Vector3.zero]);
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        ILeavesRegistry<MusicLeaf> musicsRegistry = registryResolver.Resolve<MusicLeaf>();
        MusicWhenInRange = new(musicsRegistry.LeavesByGameIds[InternalData[2]]);
    }
}