using UnityEngine;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects;

public sealed class FlytrapPlatformMapEntityLeaf : MapEntityLeaf
{
    internal FlytrapPlatformMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.TempPlatform;
    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

    public Vector3 StartingPosition { get => InternalStartingPosition; set => InternalStartingPosition = value; }
    public Vector3 EulerAngles { get => InternalEulerAngles; set => InternalEulerAngles = value; }

    public int TimeInFramesPlayerCanStayOnPlatform
    {
        get => InternalData[0].Value;
        set => InternalData[0].Value = value;
    }

    public float DelayFramesBeforeRespawnWhenFlytrapCloses
    {
        get => InternalVectorData[0].Value.x;
        set => InternalVectorData[0].Value.x = value;
    }

    internal override void InitializeFromNew()
    {
        InternalData.AddRange([new(60), new(1), new(1), new(1)]);
        InternalVectorData.Add(new(new(60f, 0f, 0f)));
        InternalAnimIdOrItemId = (int)MainManager.AnimIDs.FlyTrapPlatform - 1;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver) { }
}