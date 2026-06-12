using UnityEngine;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.Crystals;

public abstract class CrystalMapEntityLeaf : ObjectMapEntityLeaf
{
    protected CrystalMapEntityLeaf(int gameId, string namedId, string creatorId) : base(gameId, namedId, creatorId)
    {
    }

    internal sealed override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.SavePoint;

    public bool HasAnIlluminatedSphere
    {
        get => InternalData[0].Value == 1;
        set => InternalData[0].Value = value ? 1 : 0;
    }

    protected void InitializeFromNew(Vector3 startingPosition)
    {
        EntityStartingPosition = startingPosition;
        InternalVectorData.Add(new(Vector3.zero));
        InternalAnimIdOrItemId = (int)MainManager.AnimIDs.SavePoint - 1;
    }

    internal sealed override void InitializeFromExisting(IRegistryResolver registryResolver) { }
}