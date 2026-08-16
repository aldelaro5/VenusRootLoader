using UnityEngine;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.CuttableGrasses;

public abstract class CuttableGrassMapEntityLeaf : ObjectMapEntityLeaf
{
    protected CuttableGrassMapEntityLeaf(int gameId, string creatorId, string namedId) : base(
        gameId,
        creatorId,
        namedId)
    {
    }

    internal sealed override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.BeetleGrass;

    public Vector3 TriggerBoxColliderCenter { get => InternalBoxColCenter; set => InternalBoxColCenter = value; }
    public Vector3 TriggerBoxColliderSize { get => InternalBoxColSize; set => InternalBoxColSize = value; }

    public Branch<CuttableGrassLeaf> Grass
    {
        get;
        set
        {
            InternalData[0].Value = value.Resolve().GameId;
            field = value;
        }
    } = null!;

    protected void InitializeFromNew(Vector3 startingPosition, Branch<CuttableGrassLeaf> grass)
    {
        InternalData.Add(new(0));
        InternalHaxBoxCol = true;
        InternalBoxColIsTrigger = false;
        InternalBoxColCenter = new(0f, 10f, 0f);
        InternalBoxColSize = new(1.5f, 20f, 0.75f);
        EntityStartingPosition = startingPosition;
        Grass = grass;
    }
}