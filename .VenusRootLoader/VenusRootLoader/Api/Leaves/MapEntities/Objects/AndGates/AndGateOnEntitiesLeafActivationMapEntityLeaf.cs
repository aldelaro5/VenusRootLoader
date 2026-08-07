using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;
using VenusRootLoader.SourceGenerators;

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
            InternalData[0].Value = value?.Resolve().GameId ?? -1;
            field = value;
        }
    }

    [MapEntityInitializeFromNew]
    internal void InitializeFromNew(IList<NegatableMapEntityActivation> entityActivationsInputs)
    {
        base.InitializeFromNew();
        InternalData.AddRange([new(-1)]);
        foreach (NegatableMapEntityActivation entityActivationsInput in entityActivationsInputs)
            EntityActivationsInputs.Add(entityActivationsInput);
    }

    internal override void InitializeFromExisting()
    {
        if (InternalData[0].Value > -1)
        {
            ILeavesRegistry<EventLeaf> eventsRegistry = RegistryResolver.Resolve<EventLeaf>();
            OneShotEventOutputOverride = new(eventsRegistry.GetByGameId(InternalData[0].Value));
        }

        MapLeaf map = RegistryResolver.Resolve<MapLeaf>().GetByGameId(Map.Resolve().GameId);
        _entityActivationsInputs.SynchronizeFromExistingData(
            InternalData
                .Skip(1)
                .Select(x => new NegatableMapEntityActivation
                {
                    MapEntity = new((ObjectMapEntityLeaf)map.EntitiesRegistry.GetByGameId(Math.Abs(x.Value))),
                    IsActivationValueNegated = x.Value < 0
                })
                .ToList());
    }
}