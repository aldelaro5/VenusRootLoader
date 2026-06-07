using UnityEngine;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.CuttableGrasses;

public abstract class CuttableGrassMapEntityLeaf : ObjectMapEntityLeaf
{
    protected CuttableGrassMapEntityLeaf(int gameId, string namedId, string creatorId) : base(
        gameId,
        namedId,
        creatorId)
    {
    }

    internal sealed override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.BeetleGrass;

    public Vector3 TriggerBoxColliderCenter { get => InternalBoxColCenter; set => InternalBoxColCenter = value; }
    public Vector3 TriggerBoxColliderSize { get => InternalBoxColSize; set => InternalBoxColSize = value; }

    // TODO: Move this to a leaf
    public int GrassSpriteId { get => InternalData[0].Value; set => InternalData[0].Value = value; }

    internal override void InitializeFromNew()
    {
        InternalHaxBoxCol = true;
        InternalBoxColIsTrigger = false;
        InternalBoxColCenter = new(0f, 10f, 0f);
        InternalBoxColSize = new(1.5f, 20f, 0.75f);
    }
}