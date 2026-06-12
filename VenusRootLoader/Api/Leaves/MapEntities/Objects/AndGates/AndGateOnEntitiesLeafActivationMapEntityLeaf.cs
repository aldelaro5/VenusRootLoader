using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.AndGates;

public sealed class AndGateOnEntitiesLeafActivationMapEntityLeaf : AndGateMapEntityLeaf
{
    internal AndGateOnEntitiesLeafActivationMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
        _entityActivationsInputs = new(InternalData, 1, x => x.IntRef);
    }

    private readonly ListRefWrapper<NegatableMapEntityActivation, int> _entityActivationsInputs;
    public IList<NegatableMapEntityActivation> EntityActivationsInputs => _entityActivationsInputs;

    public Branch<EventLeaf>? OneShotEventOutputOverride
    {
        get;
        set
        {
            InternalData[0].Value = value?.GameId ?? -1;
            field = value;
        }
    }

    internal void InitializeFromNew(IList<NegatableMapEntityActivation> entityActivationsInputs)
    {
        base.InitializeFromNew();
        InternalData.AddRange([new(-1)]);
        foreach (NegatableMapEntityActivation entityActivationsInput in entityActivationsInputs)
            EntityActivationsInputs.Add(entityActivationsInput);
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        if (InternalData[0].Value > -1)
        {
            ILeavesRegistry<EventLeaf> eventsRegistry = registryResolver.Resolve<EventLeaf>();
            OneShotEventOutputOverride = new(eventsRegistry.LeavesByGameIds[InternalData[0].Value]);
        }

        MapLeaf map = registryResolver.Resolve<MapLeaf>().LeavesByGameIds[Map.GameId];
        _entityActivationsInputs.SynchronizeFromExistingData(
            InternalData
                .Skip(1)
                .Select(x => new NegatableMapEntityActivation
                {
                    MapEntity = (Branch<ObjectMapEntityLeaf>)map.EntitiesRegistry.LeavesByGameIds[Math.Abs(x.Value)]!,
                    IsActivationValueNegated = x.Value < 0
                })
                .ToList());
    }
}