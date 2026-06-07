using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects;

public sealed class GeyserMapEntityLeaf : ObjectMapEntityLeaf
{
    internal GeyserMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.Geizer;

    public bool IsHoneyGeyser { get => InternalData[0].Value == 1; set => InternalData[0].Value = value ? 1 : 0; }

    public Branch<MapEntityLeaf>? MapEntityActivationRequiredToBeActive
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

    public float TimeInFramesFrozenWhenFrozen
    {
        get => InternalVectorData[0].Value.z;
        set => InternalVectorData[0].Value.z = value;
    }

    internal override void InitializeFromNew()
    {
        InternalData.AddRange([new(-1), new(-1), new(-1), new(-1), new(0)]);
        InternalVectorData.Add(new(new(1f, 1f, 1000f)));
        InternalHaxBoxCol = true;
        InternalBoxColIsTrigger = true;
        InternalBoxColSize = new(2f, 6f, 2f);
        InternalBoxColCenter = new(0f, 3f, 0f);
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalData.Count < 3)
            InternalData.AddRange(Enumerable.Repeat(new Ref<int>(-1), 3 - InternalData.Count));
        if (InternalData.Count < 5)
            InternalData.AddRange(Enumerable.Repeat(new Ref<int>(0), 5 - InternalData.Count));

        if (InternalData[1].Value != -1)
            MapEntityActivationRequiredToBeActive = Map.Leaf.EntitiesRegistry.LeavesByGameIds[InternalData[1].Value];
    }
}