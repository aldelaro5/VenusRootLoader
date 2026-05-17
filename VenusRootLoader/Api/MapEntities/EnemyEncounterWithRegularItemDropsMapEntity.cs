using CommunityToolkit.Diagnostics;
using System.Collections.ObjectModel;
using System.Reflection;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Api.MapEntities.ActionBehaviors;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.MapEntities;

using ItemDrop = (Branch<ItemLeaf> Item, Branch<FlagLeaf>? RequiredFlag);

// TODO: Abstract the action behaviors logic somewhere so it can be shared
public sealed class EnemyEncounterWithRegularItemDropsMapEntity : MapEntity
{
    internal override NPCControl.NPCType Type => NPCControl.NPCType.Enemy;
    internal override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.None;

    public Vector3 StartingPosition { get => InternalStartingPosition; set => InternalStartingPosition = value; }

    public Branch<AnimIdLeaf> AnimId
    {
        get;
        set
        {
            InternalAnimIdOrItemId = value.GameId;
            field = value;
        }
    }

    public ReadOnlyCollection<Branch<EnemyLeaf>> EnemiesFormationInBattle { get; private set; } =
        new List<Branch<EnemyLeaf>>().AsReadOnly();

    public ReadOnlyCollection<ItemDrop> ItemsDropPoolWhenDefeated { get; private set; } =
        new List<ItemDrop>().AsReadOnly();

    public float MovementRadius
    {
        get => InternalRadiusLimit;
        set => InternalRadiusLimit = value;
    }

    internal EnemyEncounterWithRegularItemDropsMapEntity() { }

    internal override void InitializeFromNew()
    {
        InternalBattleEnemyIds.Add(0);
        InternalAnimIdOrItemId = 0;
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        ILeavesRegistry<EnemyLeaf> enemiesRegistry = registryResolver.Resolve<EnemyLeaf>();
        ILeavesRegistry<ItemLeaf> itemsRegistry = registryResolver.Resolve<ItemLeaf>();
        ILeavesRegistry<FlagLeaf> flagsRegistry = registryResolver.Resolve<FlagLeaf>();
        ILeavesRegistry<AnimIdLeaf> animIdsRegistry = registryResolver.Resolve<AnimIdLeaf>();

        InitializeExistingBehavior(ActionBehaviorKind.OutOfRange, registryResolver);
        InitializeExistingBehavior(ActionBehaviorKind.InRange, registryResolver);

        AnimId = new(animIdsRegistry.LeavesByGameIds[InternalAnimIdOrItemId]);

        List<Branch<EnemyLeaf>> enemies = InternalBattleEnemyIds
            .Select(i => new Branch<EnemyLeaf>(enemiesRegistry.LeavesByGameIds[i]))
            .ToList();
        ChangeEnemiesFormationInBattle(enemies);

        List<ItemDrop> itemsDrop = InternalVectorData
            .Select(itemDrop =>
            {
                Branch<ItemLeaf> item = new(itemsRegistry.LeavesByGameIds[(int)itemDrop.x]);
                Branch<FlagLeaf>? flag = itemDrop.y switch
                {
                    >= 0f => new(flagsRegistry.LeavesByGameIds[(int)itemDrop.y]),
                    _ => null
                };
                return (item, flag);
            })
            .ToList();
        ChangeItemsDropPoolWhenDefeated(itemsDrop);
    }

    private void InitializeExistingBehavior(ActionBehaviorKind kind, IRegistryResolver registryResolver)
    {
        NPCControl.ActionBehaviors behaviorType = kind switch
        {
            ActionBehaviorKind.OutOfRange => InternalPrimaryBehavior,
            ActionBehaviorKind.InRange => InternalSecondaryBehavior,
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<NPCControl.ActionBehaviors>(nameof(kind))
        };

        ActionBehavior behavior = behaviorType switch
        {
            NPCControl.ActionBehaviors.FacePlayer => new FacePlayerActionBehavior(this, kind),
            _ => new BlankActionBehavior(this, kind)
        };

        behavior.InitializeFromExisting(registryResolver);
    }

    public void ChangeEnemiesFormationInBattle(List<Branch<EnemyLeaf>> enemies)
    {
        InternalBattleEnemyIds.Clear();
        InternalBattleEnemyIds.AddRange(enemies.Select(t => t.GameId));
        EnemiesFormationInBattle = enemies.AsReadOnly();
    }

    public void ChangeItemsDropPoolWhenDefeated(List<ItemDrop> items)
    {
        InternalVectorData.Clear();
        foreach (ItemDrop itemDrop in items)
        {
            int itemGameId = itemDrop.Item.GameId;
            int requiredFlagGameId = itemDrop.RequiredFlag?.GameId ?? -1;
            InternalVectorData.Add(new(itemGameId, requiredFlagGameId, 0f));
        }

        ItemsDropPoolWhenDefeated = items.AsReadOnly();
    }

    public TBehavior SetOutOfRangeBehavior<TBehavior>() where TBehavior : ActionBehavior
    {
        TBehavior behavior = (TBehavior)Activator.CreateInstance(
            typeof(TBehavior),
            BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.NonPublic,
            null,
            [this, ActionBehaviorKind.OutOfRange],
            null,
            null);

        InternalPrimaryBehavior = behavior.BehaviorType;
        return behavior;
    }

    public TBehavior SetInRangeBehavior<TBehavior>() where TBehavior : ActionBehavior
    {
        TBehavior behavior = (TBehavior)Activator.CreateInstance(
            typeof(TBehavior),
            BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.NonPublic,
            null,
            [this, ActionBehaviorKind.InRange],
            null,
            null);

        InternalSecondaryBehavior = behavior.BehaviorType;
        return behavior;
    }
}