using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves.MapEntities.Enemies;

public sealed class EnemyEncounterDroppingItemsMapEntityLeaf : EnemyEncounterMapEntityLeaf
{
    internal EnemyEncounterDroppingItemsMapEntityLeaf(int gameId, string creatorId, string namedId)
        : base(gameId, creatorId, namedId)
    {
        _itemsDropPoolWhenDefeated = new(InternalVectorData, 0, x => x.Vector3Ref);
    }

    public NPCControl.DeathType DefeatAnimation { get => InternalDeathType; set => InternalDeathType = value; }

    private readonly ListRefWrapper<EnemyItemDrop, Vector3> _itemsDropPoolWhenDefeated;
    public IList<EnemyItemDrop> ItemsDropPoolWhenDefeated => _itemsDropPoolWhenDefeated;

    [MapEntityInitializeFromNew]
    internal void InitializeFromNew(
        Vector3 startingPosition,
        Branch<AnimIdLeaf> animId,
        IList<Branch<HasEnemyLeaf>> enemiesFormationInBattle,
        IList<EnemyItemDrop> itemsDropPoolWhenDefeated)
    {
        base.InitializeFromNew(startingPosition, animId, enemiesFormationInBattle);
        foreach (EnemyItemDrop enemyItemDrop in itemsDropPoolWhenDefeated)
            ItemsDropPoolWhenDefeated.Add(enemyItemDrop);
        DefeatAnimation = NPCControl.DeathType.SpinSmoke;
    }

    internal override void InitializeFromExisting()
    {
        base.InitializeFromExisting();
        ILeavesRegistry<ItemLeaf> itemsRegistry = RegistryResolver.Resolve<ItemLeaf>();
        ILeavesRegistry<FlagLeaf> flagsRegistry = RegistryResolver.Resolve<FlagLeaf>();

        _itemsDropPoolWhenDefeated.SynchronizeFromExistingData(
            InternalVectorData.Select(itemDrop => new EnemyItemDrop
            {
                Item = itemsRegistry.GetByGameId((int)itemDrop.Value.x),
                RequiredFlag = itemDrop.Value.y switch
                {
                    >= 0f => new(flagsRegistry.GetByGameId((int)itemDrop.Value.y)),
                    _ => null
                }
            }).ToList());
    }
}