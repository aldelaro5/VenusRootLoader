using UnityEngine;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects;

public sealed class FlytrapPlatformMapEntityLeaf : ObjectMapEntityLeaf
{
    internal FlytrapPlatformMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.TempPlatform;

    public int TimeInFramesPlayerCanStayOnPlatformBeforeFlyTrapCloses
    {
        get => InternalData[0].Value;
        set => InternalData[0].Value = value;
    }

    public float DelayFramesBeforeRespawnWhenFlytrapCloses
    {
        get => InternalVectorData[0].Value.x;
        set => InternalVectorData[0].Value.x = value;
    }

    internal void InitializeFromNew(Vector3 startingPosition)
    {
        InternalData.AddRange([new(60), new(1), new(1), new(1)]);
        InternalVectorData.Add(new(new(60f, 0f, 0f)));
        InternalAnimIdOrItemId = (int)MainManager.AnimIDs.FlyTrapPlatform - 1;
        EntityStartingPosition = startingPosition;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver) { }
}