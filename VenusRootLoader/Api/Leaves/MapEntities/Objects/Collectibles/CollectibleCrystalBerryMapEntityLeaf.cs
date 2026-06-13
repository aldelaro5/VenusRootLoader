using UnityEngine;
using VenusRootLoader.Registry;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.Collectibles;

public sealed class CollectibleCrystalBerryMapEntityLeaf : CollectibleMapEntityLeaf
{
    internal CollectibleCrystalBerryMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    public Branch<CrystalBerryLeaf> CrystalBerry
    {
        get;
        set
        {
            InternalData[3].Value = value.GameId;
            field = value;
        }
    }

    [MapEntityInitializeFromNew]
    internal void InitializeFromNew(Vector3 startingPosition, Branch<CrystalBerryLeaf> crystalBerry)
    {
        base.InitializeFromNew(startingPosition);
        InternalData.AddRange([new(3), new(-1), new(0), new(0)]);
        CrystalBerry = crystalBerry;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        base.InitializeFromExisting(registryResolver);
        ILeavesRegistry<CrystalBerryLeaf> crystalBerriesRegistry = registryResolver.Resolve<CrystalBerryLeaf>();

        CrystalBerry = new(crystalBerriesRegistry.LeavesByGameIds[InternalData[3].Value]);
    }
}