using UnityEngine;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects;

public sealed class MusicChangeRadiusMapEntityLeaf : ObjectMapEntityLeaf
{
    internal MusicChangeRadiusMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.MusicRange;

    public int FramesDelayBeforeMusicChange { get => InternalData[1].Value; set => InternalData[1].Value = value; }

    public Branch<MusicLeaf> MusicWhenInRange
    {
        get;
        set
        {
            InternalData[2].Value = value.GameId;
            field = value;
        }
    }

    public float RangeRadius
    {
        get => InternalVectorData[0].Value.x;
        set => InternalVectorData[0].Value.x = value;
    }

    public float MusicFadeRate
    {
        get => InternalVectorData[0].Value.y;
        set => InternalVectorData[0].Value.y = value;
    }

    public float MusicMaxVolumeMultiplier
    {
        get => InternalVectorData[0].Value.z;
        set => InternalVectorData[0].Value.z = value;
    }

    internal void InitializeFromNew(
        Vector3 startingPosition,
        Branch<MusicLeaf> musicWhenInRange,
        float rangeRadiusFromPosition)
    {
        InternalData.AddRange([new(0), new(20), new(0)]);
        InternalVectorData.AddRange([new(new(10f, 0.1f, 1f)), new(Vector3.zero)]);
        MusicWhenInRange = musicWhenInRange;
        RangeRadius = rangeRadiusFromPosition;
        EntityStartingPosition = startingPosition;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        ILeavesRegistry<MusicLeaf> musicsRegistry = registryResolver.Resolve<MusicLeaf>();
        MusicWhenInRange = new(musicsRegistry.LeavesByGameIds[InternalData[2].Value]);
    }
}