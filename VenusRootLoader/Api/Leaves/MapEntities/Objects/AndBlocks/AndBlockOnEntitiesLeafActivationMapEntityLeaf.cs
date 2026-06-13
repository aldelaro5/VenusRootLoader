using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.AndBlocks;

public sealed class AndBlockOnEntitiesLeafActivationMapEntityLeaf : AndBlockMapEntityLeaf
{
    internal AndBlockOnEntitiesLeafActivationMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
        _entityActivationsInputs = new(InternalData, 1, x => x.IntRef);
    }

    private readonly ListRefWrapper<NegatableMapEntityActivation, int> _entityActivationsInputs;
    public IList<NegatableMapEntityActivation> EntityActivationsInputs => _entityActivationsInputs;

    [MapEntityInitializeFromNew]
    internal void InitializeFromNew(
        Vector3 startingPosition,
        Branch<AnimIdLeaf>? animId,
        IList<NegatableMapEntityActivation> entityActivationsInputs)
    {
        base.InitializeFromNew(startingPosition, animId);
        InternalData.AddRange([new(-1)]);
        foreach (NegatableMapEntityActivation entityActivationInput in entityActivationsInputs)
            EntityActivationsInputs.Add(entityActivationInput);
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        base.InitializeFromExisting(registryResolver);
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