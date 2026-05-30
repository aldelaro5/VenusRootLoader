using UnityEngine;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects;

public sealed class GeyserMapEntityLeaf : MapEntityLeaf
{
    internal GeyserMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.Geizer;
    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

    public Vector3 StartingPosition { get => InternalStartingPosition; set => InternalStartingPosition = value; }
    public Vector3 EulerAngles { get => InternalEulerAngles; set => InternalEulerAngles = value; }

    public bool IsHoneyGeyser { get => InternalData[0] == 1; set => InternalData[0] = value ? 1 : 0; }

    public Branch<MapEntityLeaf>? MapEntityActivationRequiredToBeActive
    {
        get;
        set
        {
            InternalData[1] = value?.GameId ?? -1;
            field = value;
        }
    }

    // TODO: Test this later
    public bool SpoutsFromMapWaterLevel { get => InternalData[2] == 1; set => InternalData[2] = value ? 1 : 0; }

    // TODO: This seems to not work right on a regular grounded surface, recheck if it's bound to water level
    public bool IsUnseenUntilActive { get => InternalData[3] != 0; set => InternalData[3] = value ? 1 : 0; }

    public bool WontFreezeFromIceRadius { get => InternalData[4] != 0; set => InternalData[4] = value ? 1 : 0; }

    public float OscillationFrequencyInHertz
    {
        get => InternalVectorData[0].x / 6f;
        set => InternalVectorData[0] = new(value * 6f, InternalVectorData[0].y, InternalVectorData[0].z);
    }

    public float OscillationMagnitude
    {
        get => InternalVectorData[0].y;
        set => InternalVectorData[0] = new(InternalVectorData[0].x, value, InternalVectorData[0].z);
    }

    public float TimeInFramesFrozenWhenFrozen
    {
        get => InternalVectorData[0].z;
        set => InternalVectorData[0] = new(InternalVectorData[0].x, InternalVectorData[0].y, value);
    }

    internal override void InitializeFromNew()
    {
        InternalData.AddRange([-1, -1, -1, -1, 0]);
        InternalVectorData.Add(new(1f, 1f, 1000f));
        InternalHaxBoxCol = true;
        InternalBoxColIsTrigger = true;
        InternalBoxColSize = new(2f, 6f, 2f);
        InternalBoxColCenter = new(0f, 3f, 0f);
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalData.Count < 3)
            InternalData.AddRange(Enumerable.Repeat(-1, 3 - InternalData.Count));
        if (InternalData.Count < 5)
            InternalData.AddRange(Enumerable.Repeat(0, 5 - InternalData.Count));

        if (InternalData[1] != -1)
            MapEntityActivationRequiredToBeActive = Map.Leaf.EntitiesRegistry.LeavesByGameIds[InternalData[1]];
    }
}