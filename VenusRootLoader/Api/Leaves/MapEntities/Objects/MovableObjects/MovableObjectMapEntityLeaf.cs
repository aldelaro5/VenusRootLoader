using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.MovableObjects;

public abstract class MovableObjectMapEntityLeaf : ObjectMapEntityLeaf
{
    protected MovableObjectMapEntityLeaf(int gameId, string namedId, string creatorId) : base(
        gameId,
        namedId,
        creatorId)
    {
    }

    internal sealed override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.PushRock;

    internal virtual void InitializeFromNew(Vector3 startingPosition)
    {
        InternalHaxBoxCol = true;
        InternalBoxColIsTrigger = false;
        EntityStartingPosition = startingPosition;
    }

    internal sealed override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalData.Count < 4)
            InternalData.AddRange(Enumerable.Repeat(new Ref<int>(0), 4 - InternalData.Count));
        if (InternalVectorData.Count < 2)
        {
            InternalVectorData.AddRange(
                Enumerable.Repeat(new Ref<Vector3>(Vector3.zero), 2 - InternalVectorData.Count));
        }
    }
}