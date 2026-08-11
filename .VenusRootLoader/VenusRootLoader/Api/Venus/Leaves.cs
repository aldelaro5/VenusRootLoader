using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Api.Leaves.MapEntities;
using VenusRootLoader.Api.Unity;
using VenusRootLoader.Registry;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global

namespace VenusRootLoader.Api;

public partial class Venus
{
    // These registry methods are too specialized to be source generated

    public HasEnemyLeaf RegisterSpyableEnemy(
        string namedId,
        Branch<AnimIdLeaf> entityAnimId,
        IAssetLoader<Sprite> portraitSprite,
        MainManager.Enemies? orderAfterInBestiary,
        int orderPriorityInBestiary)
    {
        HasEnemyLeaf hasEnemyLeaf = RegistryResolver.ResolveWithOrdering<HasEnemyLeaf>().RegisterNewWithOrdering(
            BudId,
            namedId,
            (int?)orderAfterInBestiary,
            orderPriorityInBestiary);
        hasEnemyLeaf.CanBeSpied = true;
        hasEnemyLeaf.EntityAnimId = entityAnimId;
        hasEnemyLeaf.PortraitSprite = portraitSprite;
        return hasEnemyLeaf;
    }

    public HasEnemyLeaf RegisterNonSpyableEnemy(
        string namedId,
        Branch<AnimIdLeaf> entityAnimId)
    {
        HasEnemyLeaf hasEnemyLeaf = RegistryResolver.Resolve<HasEnemyLeaf>().RegisterNew(BudId, namedId);
        hasEnemyLeaf.CanBeSpied = false;
        hasEnemyLeaf.EntityAnimId = entityAnimId;
        return hasEnemyLeaf;
    }

    public SpyCardLeaf RegisterSpyCard(
        string namedId,
        Branch<HasEnemyLeaf> enemy,
        int tpCost,
        CardGame.Type type,
        MainManager.Enemies? orderAfter,
        int orderPriority)
    {
        IOrderedLeavesRegistry<SpyCardLeaf> orderedLeavesRegistry = RegistryResolver.ResolveWithOrdering<SpyCardLeaf>();
        int? gameIdOrderAfter;
        if (orderAfter is not null)
        {
            gameIdOrderAfter = orderedLeavesRegistry.Registry
                .OrderBy(l => l.GameId)
                .First(l => l.Enemy.Resolve().GameId == (int)orderAfter).GameId;
        }
        else
        {
            gameIdOrderAfter = null;
        }

        SpyCardLeaf leaf = orderedLeavesRegistry.RegisterNewWithOrdering(
            namedId,
            BudId,
            gameIdOrderAfter,
            orderPriority);
        leaf.InitializeFromNew(enemy, tpCost, type);
        return leaf;
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

    public bool TryGetMapEntity(string creatorId, string namedId, MapLeaf map, out MapEntityLeaf? mapEntityLeaf) =>
        map.EntitiesRegistry.TryGet(creatorId, namedId, out mapEntityLeaf);

    public MapEntityLeaf GetMapEntityFromBaseGame(string namedId, MapLeaf map) =>
        map.EntitiesRegistry.Get(Constants.BaseGameCreatorId, namedId);

    public IReadOnlyCollection<MapEntityLeaf> GetAllMapEntities(MapLeaf map) =>
        map.EntitiesRegistry.GetAll();
}