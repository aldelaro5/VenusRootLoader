using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.AndBlocks;

public sealed class AndBlockOnEntitiesLeafActivationMapEntityLeaf : AndBlockMapEntityLeaf
{
    internal AndBlockOnEntitiesLeafActivationMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
        _entityActivationsInput = new(InternalData, 1, x => x.Ref);
    }

    private readonly ListRefWrapper<NegatableMapEntityActivation, int> _entityActivationsInput;
    public IList<NegatableMapEntityActivation> EntityActivationsInput => _entityActivationsInput;

    internal override void InitializeFromNew()
    {
        base.InitializeFromNew();
        InternalData.AddRange([new(-1)]);
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        base.InitializeFromExisting(registryResolver);
        MapLeaf map = registryResolver.Resolve<MapLeaf>().LeavesByGameIds[Map.GameId];

        _entityActivationsInput.SynchronizeFromExistingData(
            InternalData
                .Skip(1)
                .Select(x => new NegatableMapEntityActivation
                {
                    MapEntity = map.EntitiesRegistry.LeavesByGameIds[Math.Abs(x.Value)],
                    IsActivationValueNegated = x.Value < 0
                })
                .ToList());
    }
}