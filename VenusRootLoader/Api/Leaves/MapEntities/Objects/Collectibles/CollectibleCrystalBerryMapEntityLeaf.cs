using VenusRootLoader.Registry;

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

    internal override void InitializeFromNew()
    {
        InternalData.AddRange([new(3), new(-1), new(0), new(0)]);
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        base.InitializeFromExisting(registryResolver);
        ILeavesRegistry<CrystalBerryLeaf> crystalBerriesRegistry = registryResolver.Resolve<CrystalBerryLeaf>();

        CrystalBerry = new(crystalBerriesRegistry.LeavesByGameIds[InternalData[3].Value]);
    }
}