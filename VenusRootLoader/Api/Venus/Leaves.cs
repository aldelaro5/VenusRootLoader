using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Api.Leaves.MapEntities;
using VenusRootLoader.Registry;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global

namespace VenusRootLoader.Api;

public partial class Venus
{
    // TODO: Move all of this to a source generator
    // public MedalLeaf RegisterMedal(string namedId, MainManager.BadgeTypes? orderAfter, int orderPriority) =>
    //     RegistryResolver.ResolveWithOrdering<MedalLeaf>().RegisterNewWithOrdering(
    //         namedId,
    //         BudId,
    //         (int?)orderAfter,
    //         orderPriority);
    //
    // public MedalLeaf GetMedal(string namedId) =>
    //     RegistryResolver.Resolve<MedalLeaf>().Get(namedId);
    //
    // public IReadOnlyCollection<MedalLeaf> GetAllMedals() =>
    //     RegistryResolver.Resolve<MedalLeaf>().GetAll();
    //
    // public DiscoveryLeaf RegisterDiscovery(string namedId, int? orderAfter, int orderPriority) =>
    //     RegistryResolver.ResolveWithOrdering<DiscoveryLeaf>().RegisterNewWithOrdering(
    //         namedId,
    //         BudId,
    //         orderAfter,
    //         orderPriority);
    //
    // public DiscoveryLeaf GetDiscovery(string namedId) =>
    //     RegistryResolver.Resolve<DiscoveryLeaf>().Get(namedId);
    //
    // public IReadOnlyCollection<DiscoveryLeaf> GetAllDiscoveries() =>
    //     RegistryResolver.Resolve<DiscoveryLeaf>().GetAll();
    //
    // public RecordLeaf RegisterRecord(string namedId, int? orderAfter, int orderPriority) =>
    //     RegistryResolver.ResolveWithOrdering<RecordLeaf>().RegisterNewWithOrdering(
    //         namedId,
    //         BudId,
    //         orderAfter,
    //         orderPriority);
    //
    // public RecordLeaf GetRecord(string namedId) =>
    //     RegistryResolver.Resolve<RecordLeaf>().Get(namedId);
    //
    // public IReadOnlyCollection<RecordLeaf> GetAllRecords() =>
    //     RegistryResolver.Resolve<RecordLeaf>().GetAll();
    //
    // public EnemyLeaf RegisterSpyableEnemy(
    //     string namedId,
    //     MainManager.Enemies? orderAfterInBestiary,
    //     int orderPriorityInBestiary)
    // {
    //     EnemyLeaf enemyLeaf = RegistryResolver.ResolveWithOrdering<EnemyLeaf>().RegisterNewWithOrdering(
    //         namedId,
    //         BudId,
    //         (int?)orderAfterInBestiary,
    //         orderPriorityInBestiary);
    //     enemyLeaf.CanBeSpied = true;
    //     return enemyLeaf;
    // }
    //
    // public EnemyLeaf RegisterNonSpyableEnemy(string namedId)
    // {
    //     EnemyLeaf enemyLeaf = RegistryResolver.Resolve<EnemyLeaf>().RegisterNew(namedId, BudId);
    //     enemyLeaf.CanBeSpied = false;
    //     return enemyLeaf;
    // }
    //
    // public EnemyLeaf GetEnemy(string namedId) =>
    //     RegistryResolver.Resolve<EnemyLeaf>().Get(namedId);
    //
    // public IReadOnlyCollection<EnemyLeaf> GetAllEnemies() =>
    //     RegistryResolver.Resolve<EnemyLeaf>().GetAll();
    //
    // public SpyCardLeaf RegisterSpyCard(string namedId, MainManager.Enemies? orderAfter, int orderPriority)
    // {
    //     IOrderedLeavesRegistry<SpyCardLeaf> orderedLeavesRegistry = RegistryResolver.ResolveWithOrdering<SpyCardLeaf>();
    //     int? gameIdOrderAfter;
    //     if (orderAfter is not null)
    //     {
    //         gameIdOrderAfter = orderedLeavesRegistry.Registry.LeavesByGameIds.Values
    //             .OrderBy(l => l.GameId)
    //             .First(l => l.Enemy.GameId == (int)orderAfter).GameId;
    //     }
    //     else
    //     {
    //         gameIdOrderAfter = null;
    //     }
    //
    //     return orderedLeavesRegistry.RegisterNewWithOrdering(
    //         namedId,
    //         BudId,
    //         gameIdOrderAfter,
    //         orderPriority);
    // }
    //
    // public SpyCardLeaf GetSpyCard(string namedId) =>
    //     RegistryResolver.Resolve<SpyCardLeaf>().Get(namedId);
    //
    // public IReadOnlyCollection<SpyCardLeaf> GetAllSpyCards() =>
    //     RegistryResolver.Resolve<SpyCardLeaf>().GetAll();
    //
    // public MedalShopLeaf RegisterMedalShop(string namedId, Branch<FlagLeaf> boughtAllStockFlag)
    // {
    //     MedalShopLeaf medalShopLeaf = RegistryResolver.Resolve<MedalShopLeaf>().RegisterNew(namedId, BudId);
    //     medalShopLeaf.BoughtAllStockFlag = boughtAllStockFlag;
    //     return medalShopLeaf;
    // }
    //
    // public MedalShopLeaf GetMedalShop(string namedId) =>
    //     RegistryResolver.Resolve<MedalShopLeaf>().Get(namedId);
    //
    // public IReadOnlyCollection<MedalShopLeaf> GetAllMedalShops() =>
    //     RegistryResolver.Resolve<MedalShopLeaf>().GetAll();
    //
    // public MapDialogueLeaf RegisterMapDialogue(string namedId, MapLeaf map)
    // {
    //     MapDialogueLeaf mapDialogueLeaf = map.DialoguesRegistry.RegisterNew(namedId, BudId);
    //     mapDialogueLeaf.Map = map;
    //     return mapDialogueLeaf;
    // }
    //
    // public MapDialogueLeaf GetMapDialogue(string namedId, MapLeaf map) =>
    //     map.DialoguesRegistry.Get(namedId);
    //
    // public IReadOnlyCollection<MapDialogueLeaf> GetAllMapDialogues(MapLeaf map) =>
    //     map.DialoguesRegistry.GetAll();
    //
    // public MapEntityLeaf GetMapEntity(string namedId, MapLeaf map) =>
    //     map.EntitiesRegistry.Get(namedId);
    //
    // public IReadOnlyCollection<MapEntityLeaf> GetAllMapEntities(MapLeaf map) =>
    //     map.EntitiesRegistry.GetAll();
}