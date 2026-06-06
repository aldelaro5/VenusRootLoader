using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Enemies;

public sealed class EnemyEncounterWithRegularItemDropsMapEntityLeaf : EnemyMapEntityLeaf
{
    internal EnemyEncounterWithRegularItemDropsMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
        _itemsDropPoolWhenDefeated = new(InternalVectorData);
    }

    internal NPCControl.DeathType DeathMethod { get => InternalDeathType; set => InternalDeathType = value; }

    private readonly ListWrapper<EnemyItemDrop, Vector3> _itemsDropPoolWhenDefeated;
    public IList<EnemyItemDrop> ItemsDropPoolWhenDefeated => _itemsDropPoolWhenDefeated;

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        base.InitializeFromExisting(registryResolver);
        ILeavesRegistry<ItemLeaf> itemsRegistry = registryResolver.Resolve<ItemLeaf>();
        ILeavesRegistry<FlagLeaf> flagsRegistry = registryResolver.Resolve<FlagLeaf>();

        _itemsDropPoolWhenDefeated.SynchronizeFromExistingData(
            InternalVectorData.Select(itemDrop => new EnemyItemDrop
            {
                Item = itemsRegistry.LeavesByGameIds[(int)itemDrop.x],
                RequiredFlag = itemDrop.y switch
                {
                    >= 0f => new(flagsRegistry.LeavesByGameIds[(int)itemDrop.y]),
                    _ => null
                }
            }).ToList());
    }
}