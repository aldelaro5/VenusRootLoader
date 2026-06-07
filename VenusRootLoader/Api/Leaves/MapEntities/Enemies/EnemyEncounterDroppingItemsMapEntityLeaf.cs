using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Enemies;

public sealed class EnemyEncounterDroppingItemsMapEntityLeaf : EnemyEncounterMapEntityLeaf
{
    internal EnemyEncounterDroppingItemsMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
        _itemsDropPoolWhenDefeated = new(InternalVectorData, 0, x => x.Vector3Ref);
    }

    internal NPCControl.DeathType DeathMethod { get => InternalDeathType; set => InternalDeathType = value; }

    private readonly ListRefWrapper<EnemyItemDrop, Vector3> _itemsDropPoolWhenDefeated;
    public IList<EnemyItemDrop> ItemsDropPoolWhenDefeated => _itemsDropPoolWhenDefeated;

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        base.InitializeFromExisting(registryResolver);
        ILeavesRegistry<ItemLeaf> itemsRegistry = registryResolver.Resolve<ItemLeaf>();
        ILeavesRegistry<FlagLeaf> flagsRegistry = registryResolver.Resolve<FlagLeaf>();

        _itemsDropPoolWhenDefeated.SynchronizeFromExistingData(
            InternalVectorData.Select(itemDrop => new EnemyItemDrop
            {
                Item = itemsRegistry.LeavesByGameIds[(int)itemDrop.Value.x],
                RequiredFlag = itemDrop.Value.y switch
                {
                    >= 0f => new(flagsRegistry.LeavesByGameIds[(int)itemDrop.Value.y]),
                    _ => null
                }
            }).ToList());
    }
}