using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Api.Leaves.MapEntities;
using VenusRootLoader.Registry;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global

namespace VenusRootLoader.Api;

public partial class Venus
{
    // These registry methods are too specialized to be source generated

    public MapLeaf RegisterMap(string namedId, Branch<AreaLeaf> area, Branch<DialogueLeaf> spyDialogue)
    {
        MapLeaf mapLeaf = RegistryResolver.Resolve<MapLeaf>().RegisterNew(BudId, namedId);
        mapLeaf.Area = area;
        mapLeaf.SpyDialogue = spyDialogue;
        mapLeaf.DialoguesRegistry = new AutoSequentialIdBasedRegistry<MapDialogueLeaf>(
            LoggerFactory.CreateLogger($"Maps.{mapLeaf.NamedId}_{nameof(MapLeaf.DialoguesRegistry)}"),
            IdSequenceDirection.Increment);
        mapLeaf.EntitiesRegistry = new AutoSequentialIdBasedRegistry<MapEntityLeaf>(
            LoggerFactory.CreateLogger($"Maps.{mapLeaf.NamedId}_{nameof(MapLeaf.EntitiesRegistry)}"),
            IdSequenceDirection.Increment);
        return mapLeaf;
    }

    public EnemyLeaf RegisterSpyableEnemy(
        string namedId,
        MainManager.Enemies? orderAfterInBestiary,
        int orderPriorityInBestiary)
    {
        EnemyLeaf enemyLeaf = RegistryResolver.ResolveWithOrdering<EnemyLeaf>().RegisterNewWithOrdering(
            BudId,
            namedId,
            (int?)orderAfterInBestiary,
            orderPriorityInBestiary);
        enemyLeaf.CanBeSpied = true;
        return enemyLeaf;
    }

    public EnemyLeaf RegisterNonSpyableEnemy(string namedId)
    {
        EnemyLeaf enemyLeaf = RegistryResolver.Resolve<EnemyLeaf>().RegisterNew(BudId, namedId);
        enemyLeaf.CanBeSpied = false;
        return enemyLeaf;
    }

    public SpyCardLeaf RegisterSpyCard(string namedId, MainManager.Enemies? orderAfter, int orderPriority)
    {
        IOrderedLeavesRegistry<SpyCardLeaf> orderedLeavesRegistry = RegistryResolver.ResolveWithOrdering<SpyCardLeaf>();
        int? gameIdOrderAfter;
        if (orderAfter is not null)
        {
            gameIdOrderAfter = orderedLeavesRegistry.Registry.LeavesByGameIds.Values
                .OrderBy(l => l.GameId)
                .First(l => l.Enemy.GameId == (int)orderAfter).GameId;
        }
        else
        {
            gameIdOrderAfter = null;
        }

        return orderedLeavesRegistry.RegisterNewWithOrdering(
            namedId,
            BudId,
            gameIdOrderAfter,
            orderPriority);
    }

    public MedalShopLeaf RegisterMedalShop(string namedId, Branch<FlagLeaf> boughtAllStockFlag)
    {
        MedalShopLeaf medalShopLeaf = RegistryResolver.Resolve<MedalShopLeaf>().RegisterNew(BudId, namedId);
        medalShopLeaf.BoughtAllStockFlag = boughtAllStockFlag;
        return medalShopLeaf;
    }

    public MapDialogueLeaf RegisterMapDialogue(string namedId, MapLeaf map)
    {
        MapDialogueLeaf mapDialogueLeaf = map.DialoguesRegistry.RegisterNew(BudId, namedId);
        mapDialogueLeaf.Map = map;
        return mapDialogueLeaf;
    }

    public MapDialogueLeaf GetMapDialogue(string creatorId, string namedId, MapLeaf map) =>
        map.DialoguesRegistry.Get(creatorId, namedId);

    public bool TryGetMapDialogue(string creatorId, string namedId, MapLeaf map, out MapDialogueLeaf? mapDialogue) =>
        map.DialoguesRegistry.TryGet(creatorId, namedId, out mapDialogue);

    public MapDialogueLeaf GetMapDialogueFromBaseGame(string namedId, MapLeaf map) =>
        map.DialoguesRegistry.Get(Constants.BaseGameCreatorId, namedId);

    public IReadOnlyCollection<MapDialogueLeaf> GetAllMapDialogues(MapLeaf map) =>
        map.DialoguesRegistry.GetAll();

    public MapEntityLeaf GetMapEntity(string creatorId, string namedId, MapLeaf map) =>
        map.EntitiesRegistry.Get(creatorId, namedId);

    public bool TryGetMapDialogue(string creatorId, string namedId, MapLeaf map, out MapEntityLeaf? mapEntityLeaf) =>
        map.EntitiesRegistry.TryGet(creatorId, namedId, out mapEntityLeaf);

    public MapEntityLeaf GetMapEntityFromBaseGame(string namedId, MapLeaf map) =>
        map.EntitiesRegistry.Get(Constants.BaseGameCreatorId, namedId);

    public IReadOnlyCollection<MapEntityLeaf> GetAllMapEntities(MapLeaf map) =>
        map.EntitiesRegistry.GetAll();
}