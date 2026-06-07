using UnityEngine;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.Crystals;

public abstract class CrystalMapEntityLeaf : MapEntityLeaf
{
    protected CrystalMapEntityLeaf(int gameId, string namedId, string creatorId) : base(gameId, namedId, creatorId)
    {
    }

    internal sealed override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal sealed override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.SavePoint;
    internal sealed override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

    public bool HasAnIlluminatedSphere
    {
        get => InternalData[0].Value == 1;
        set => InternalData[0].Value = value ? 1 : 0;
    }

    internal override void InitializeFromNew()
    {
        InternalVectorData.Add(new(Vector3.zero));
        InternalAnimIdOrItemId = (int)MainManager.AnimIDs.SavePoint - 1;
    }

    internal sealed override void InitializeFromExisting(IRegistryResolver registryResolver) { }
}