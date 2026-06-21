using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.DigSpots;

public abstract class DigSpotMapEntityLeaf : ObjectMapEntityLeaf
{
    protected DigSpotMapEntityLeaf(int gameId, string namedId, string creatorId) : base(gameId, namedId, creatorId)
    {
    }

    internal sealed override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.DigSpot;

    protected void InitializeFromNew(Vector3 startingPosition)
    {
        InternalAnimIdOrItemId = (int)MainManager.AnimIDs.DigMound - 1;
        InternalHaxBoxCol = true;
        InternalBoxColIsTrigger = true;
        InternalBoxColSize = new(1.4f, 1.4f, 1.4f);
        InternalBoxColCenter = Vector3.zero;
        EntityStartingPosition = startingPosition;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalData.Count < 3)
        {
            for (int i = 0; i < 3 - InternalData.Count; i++)
                InternalData.Add(new Ref<int>(-1));
        }
    }
}