using UnityEngine;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.CuttableGrasses;

public sealed class CuttableGrassWithCrystalBerryDropMapEntityLeaf : MapEntityLeaf
{
    internal CuttableGrassWithCrystalBerryDropMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.NPCType Type => NPCControl.NPCType.Object;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.BeetleGrass;
    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

    public Vector3 StartingPosition { get => InternalStartingPosition; set => InternalStartingPosition = value; }
    public Vector3 EulerAngles { get => InternalEulerAngles; set => InternalEulerAngles = value; }

    public Vector3 BoxColliderCenter { get => InternalBoxColCenter; set => InternalBoxColCenter = value; }
    public Vector3 BoxColliderSize { get => InternalBoxColSize; set => InternalBoxColSize = value; }

    public int GrassSpriteId { get => InternalData[0].Value; set => InternalData[0].Value = value; }

    public Branch<CrystalBerryLeaf> CrystalBerryDroppedWhenCut
    {
        get;
        set
        {
            InternalData[1].Value = value.GameId;
            field = value;
        }
    }

    internal override void InitializeFromNew()
    {
        InternalData.AddRange([new(0), new(0)]);
        InternalHaxBoxCol = true;
        InternalBoxColIsTrigger = false;
        InternalBoxColCenter = new(0f, 10f, 0f);
        InternalBoxColSize = new(1.5f, 20f, 0.75f);
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        ILeavesRegistry<CrystalBerryLeaf> crystalBerriesRegistry = registryResolver.Resolve<CrystalBerryLeaf>();
        CrystalBerryDroppedWhenCut = new(crystalBerriesRegistry.LeavesByGameIds[InternalData[1].Value]);
    }
}