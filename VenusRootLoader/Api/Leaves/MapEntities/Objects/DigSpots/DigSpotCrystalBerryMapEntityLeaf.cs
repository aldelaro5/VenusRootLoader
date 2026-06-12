using UnityEngine;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.DigSpots;

public sealed class DigSpotCrystalBerryMapEntityLeaf : DigSpotMapEntityLeaf
{
    internal DigSpotCrystalBerryMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    public Branch<CrystalBerryLeaf> CrystalBerryHiddenInside
    {
        get;
        set
        {
            InternalData[1].Value = value.GameId;
            field = value;
        }
    }

    internal void InitializeFromNew(Vector3 startingPosition, Branch<CrystalBerryLeaf> crystalBerryHiddenInside)
    {
        base.InitializeFromNew(startingPosition);
        InternalData.AddRange([new(1), new(0), new(-1)]);
        CrystalBerryHiddenInside = crystalBerryHiddenInside;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        base.InitializeFromExisting(registryResolver);
        ILeavesRegistry<CrystalBerryLeaf> crystalBerriesRegistry = registryResolver.Resolve<CrystalBerryLeaf>();
        CrystalBerryHiddenInside = new(crystalBerriesRegistry.LeavesByGameIds[InternalData[1].Value]);
    }
}