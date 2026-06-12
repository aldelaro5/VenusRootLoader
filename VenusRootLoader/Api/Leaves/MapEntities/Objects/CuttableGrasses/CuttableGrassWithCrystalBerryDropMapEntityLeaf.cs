using UnityEngine;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.CuttableGrasses;

public sealed class CuttableGrassWithCrystalBerryDropMapEntityLeaf : CuttableGrassMapEntityLeaf
{
    internal CuttableGrassWithCrystalBerryDropMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    public Branch<CrystalBerryLeaf> CrystalBerryDroppedWhenCut
    {
        get;
        set
        {
            InternalData[1].Value = value.GameId;
            field = value;
        }
    }

    internal void InitializeFromNew(Vector3 startingPosition, Branch<CrystalBerryLeaf> crystalBerryDroppedWhenCut)
    {
        base.InitializeFromNew(startingPosition);
        InternalData.AddRange([new(0), new(0)]);
        CrystalBerryDroppedWhenCut = crystalBerryDroppedWhenCut;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        ILeavesRegistry<CrystalBerryLeaf> crystalBerriesRegistry = registryResolver.Resolve<CrystalBerryLeaf>();
        CrystalBerryDroppedWhenCut = new(crystalBerriesRegistry.LeavesByGameIds[InternalData[1].Value]);
    }
}