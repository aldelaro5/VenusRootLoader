using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects;

public sealed class GeyserMapEntityLeaf : ObjectMapEntityLeaf
{
    internal GeyserMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.Geizer;

    public bool IsHoneyGeyser { get => InternalData[0].Value == 1; set => InternalData[0].Value = value ? 1 : 0; }

    public Branch<ObjectMapEntityLeaf>? MapEntityActivationRequiredToBeActive
    {
        get;
        set
        {
            InternalData[1].Value = value?.GameId ?? -1;
            field = value;
        }
    }

    // TODO: Test this later
    public bool SpoutsFromMapWaterLevel
    {
        get => InternalData[2].Value == 1;
        set => InternalData[2].Value = value ? 1 : 0;
    }

    // TODO: This seems to not work right on a regular grounded surface, recheck if it's bound to water level
    public bool IsUnseenUntilActive { get => InternalData[3].Value != 0; set => InternalData[3].Value = value ? 1 : 0; }

    public bool WontFreezeFromIceRadius
    {
        get => InternalData[4].Value != 0;
        set => InternalData[4].Value = value ? 1 : 0;
    }

    public float OscillationFrequencyInHertz
    {
        get => InternalVectorData[0].Value.x / 6f;
        set => InternalVectorData[0].Value.x = value * 6f;
    }

    public float OscillationMagnitude
    {
        get => InternalVectorData[0].Value.y;
        set => InternalVectorData[0].Value.y = value;
    }

    public float FreezeTimeInFramesWhenFrozen
    {
        get => InternalVectorData[0].Value.z;
        set => InternalVectorData[0].Value.z = value;
    }

    [MapEntityInitializeFromNew]
    internal void InitializeFromNew(
        Vector3 startingPosition,
        float oscillationFrequencyInHertz,
        float oscillationMagnitude)
    {
        InternalData.AddRange([new(-1), new(-1), new(-1), new(-1), new(0)]);
        InternalVectorData.Add(new(new(1f, 1f, 1000f)));
        InternalHaxBoxCol = true;
        InternalBoxColIsTrigger = true;
        InternalBoxColSize = new(2f, 6f, 2f);
        InternalBoxColCenter = new(0f, 3f, 0f);
        OscillationFrequencyInHertz = oscillationFrequencyInHertz;
        OscillationMagnitude = oscillationMagnitude;
        EntityStartingPosition = startingPosition;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalData.Count < 3)
        {
            for (int i = 0; i < 3 - InternalData.Count; i++)
                InternalData.Add(new Ref<int>(-1));
        }

        if (InternalData.Count < 5)
        {
            for (int i = 0; i < 5 - InternalData.Count; i++)
                InternalData.Add(new Ref<int>(0));
        }

        if (InternalData[1].Value != -1)
        {
            MapEntityActivationRequiredToBeActive =
                (Branch<ObjectMapEntityLeaf>?)Map.Leaf.EntitiesRegistry.LeavesByGameIds[InternalData[1].Value]!;
        }
    }
}