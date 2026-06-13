using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;
using VenusRootLoader.SourceGenerators;

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

    [MapEntityInitializeFromNew]
    internal void InitializeFromNew(
        Vector3 startingPosition,
        Branch<AnimIdLeaf> animId,
        IList<Branch<EnemyLeaf>> enemiesFormationInBattle,
        IList<EnemyItemDrop> itemsDropPoolWhenDefeated)
    {
        base.InitializeFromNew(startingPosition, animId, enemiesFormationInBattle);
        foreach (EnemyItemDrop enemyItemDrop in itemsDropPoolWhenDefeated)
            ItemsDropPoolWhenDefeated.Add(enemyItemDrop);
    }

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