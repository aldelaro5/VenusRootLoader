using CommunityToolkit.Diagnostics;
using System.Collections.ObjectModel;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Api.MapEntities.ActionBehaviors;
using VenusRootLoader.Api.MapEntities.ActionBehaviors.Enums;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.MapEntities;

using ItemDrop = (Branch<ItemLeaf> Item, Branch<FlagLeaf>? RequiredFlag);

// TODO: Abstract the action behaviors logic somewhere so it can be shared
// TODO: When doing NPC, do not forget to expose StealthAI, it can't be used for enemies due to conflicts with battleids
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

    public ActionBehavior? OutOfBehaviorRangeBehavior { get; private set; }
    public ActionBehavior? InBehaviorRangeBehavior { get; private set; }

    public float BehaviorRangeRadius
    {
        get => InternalRadius;
        set => InternalRadius = value;
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

        InitializeBehaviorFromExisting(ActionBehaviorKind.OutOfRange);
        InitializeBehaviorFromExisting(ActionBehaviorKind.InRange);

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

    private void InitializeBehaviorFromExisting(ActionBehaviorKind kind)
    {
        NPCControl.ActionBehaviors internalType = kind switch
        {
            ActionBehaviorKind.OutOfRange => InternalPrimaryBehavior,
            ActionBehaviorKind.InRange => InternalSecondaryBehavior,
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<NPCControl.ActionBehaviors>(nameof(kind))
        };

        ActionBehavior? behavior = MapExistingInternalBehaviorType(kind, internalType);

        switch (kind)
        {
            case ActionBehaviorKind.OutOfRange:
                OutOfBehaviorRangeBehavior = behavior;
                break;
            case ActionBehaviorKind.InRange:
                InBehaviorRangeBehavior = behavior;
                break;
            default:
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(kind));
                break;
        }
    }

    private ActionBehavior? MapExistingInternalBehaviorType(
        ActionBehaviorKind kind,
        NPCControl.ActionBehaviors internalType)
    {
        return internalType switch
        {
            NPCControl.ActionBehaviors.None => null,
            NPCControl.ActionBehaviors.FacePlayer => new FaceDirectionActionBehavior(this, kind),
            NPCControl.ActionBehaviors.ChasePlayer => new ChasePlayerActionBehavior(this, kind),
            NPCControl.ActionBehaviors.FleeFromPlayer => new FleeFromPlayerActionBehavior(this, kind),
            NPCControl.ActionBehaviors.TurnRandomly => new SpriteFlipActionBehavior(this, kind),
            NPCControl.ActionBehaviors.Wander => new WanderActionBehavior(this, kind),
            NPCControl.ActionBehaviors.FaceAwayFromPlayer => new FaceDirectionActionBehavior(this, kind),
            NPCControl.ActionBehaviors.TurnFixedInterval => new SpriteFlipActionBehavior(this, kind),
            NPCControl.ActionBehaviors.Disguise => new DisguiseActionBehavior(this, kind),
            NPCControl.ActionBehaviors.DisguiseOnce => new DisguiseOnceBeforeWanderActionBehavior(this, kind),
            NPCControl.ActionBehaviors.FaceAhead => new FaceDirectionActionBehavior(this, kind),
            NPCControl.ActionBehaviors.FaceBehind => new FaceDirectionActionBehavior(this, kind),
            NPCControl.ActionBehaviors.FaceUp => new FaceDirectionActionBehavior(this, kind),
            NPCControl.ActionBehaviors.FaceDown => new FaceDirectionActionBehavior(this, kind),
            NPCControl.ActionBehaviors.SetPath => new MoveAlongPathActionBehavior(this, kind),
            NPCControl.ActionBehaviors.ChargeAtPlayer => new ChargeAtPlayerActionBehavior(this, kind),
            NPCControl.ActionBehaviors.ChargeAtPlayerFlipSprite => new ChargeAtPlayerActionBehavior(this, kind),
            NPCControl.ActionBehaviors.ShootProjectile => new ShootProjectileActionBehavior(this, kind),
            NPCControl.ActionBehaviors.ChargeAndAttack => new ChaseAndAttackPlayerActionBehavior(this, kind),
            NPCControl.ActionBehaviors.Unmoveable => new UnmovableWhenDizzyActionBehavior(this, kind),
            NPCControl.ActionBehaviors.ChargeAttackUnderground => new ChaseAndAttackPlayerActionBehavior(this, kind),
            NPCControl.ActionBehaviors.WanderUnderground => new WanderActionBehavior(this, kind),
            NPCControl.ActionBehaviors.SetPathJump => new MoveAlongPathActionBehavior(this, kind),
            NPCControl.ActionBehaviors.ChangeSpriteInRandius => new ChangeAnimstateInRadiusActionBehavior(this),
            NPCControl.ActionBehaviors.ChaseWhenAnim => new ChasePlayerWhenAnimstateIsChaseActionBehavior(this, kind),
            NPCControl.ActionBehaviors.WalkWhenAnim => new WanderWhenAnimstateIsWalkOrIdleActionBehavior(this, kind),
            NPCControl.ActionBehaviors.WanderOffscreen => new WanderActionBehavior(this, kind),
            NPCControl.ActionBehaviors.WanderNoWarp => new WanderActionBehavior(this, kind),
            NPCControl.ActionBehaviors.WanderOnWater => new WanderActionBehavior(this, kind),
            NPCControl.ActionBehaviors.ChaseOnWater => new ChasePlayerActionBehavior(this, kind),
            _ => ThrowHelper.ThrowInvalidOperationException<ActionBehavior?>(
                $"The internal action behavior type {internalType} is not supported.")
        };
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

    public FaceDirectionActionBehavior SetFaceDirectionBehavior(
        ActionBehaviorKind kind,
        FacingBehaviorDirection direction)
    {
        FaceDirectionActionBehavior behavior = new(this, kind) { FacingDirection = direction };
        SetActionBehavior(behavior, kind);
        return behavior;
    }

    public ChasePlayerActionBehavior SetChasePlayerBehavior(ActionBehaviorKind kind, bool chaseOnWater)
    {
        ChasePlayerActionBehavior behavior = new(this, kind) { ChaseOnWater = chaseOnWater };
        SetActionBehavior(behavior, kind);
        return behavior;
    }

    public ChasePlayerWhenAnimstateIsChaseActionBehavior SetChasePlayerWhenAnimstateIsChaseBehavior(
        ActionBehaviorKind kind,
        int animstateOverrideWhenNotChase)
    {
        ChasePlayerWhenAnimstateIsChaseActionBehavior behavior = new(this, kind)
        {
            AnimstateOverrideWhenNotChase = animstateOverrideWhenNotChase
        };
        SetActionBehavior(behavior, kind);
        return behavior;
    }

    public ChaseAndAttackPlayerActionBehavior SetChaseAndAttackPlayerBehavior(
        ActionBehaviorKind kind,
        float minimumDistanceFromPlayerBeforeAttacking,
        bool attacksFromUnderground)
    {
        ChaseAndAttackPlayerActionBehavior behavior = new(this, kind)
        {
            MinimumDistanceFromPlayerBeforeAttacking = minimumDistanceFromPlayerBeforeAttacking,
            AttacksFromUnderground = attacksFromUnderground
        };
        SetActionBehavior(behavior, kind);
        return behavior;
    }

    public FleeFromPlayerActionBehavior SetFleeFromPlayerBehavior(ActionBehaviorKind kind)
    {
        FleeFromPlayerActionBehavior behavior = new(this, kind);
        SetActionBehavior(behavior, kind);
        return behavior;
    }

    public SpriteFlipActionBehavior SetSpriteFlipBehavior(
        ActionBehaviorKind kind,
        float baseFlipIntervalInFrames,
        bool flipsAtRandomInterval)
    {
        SpriteFlipActionBehavior behavior = new(this, kind)
        {
            BaseFlipIntervalInFrames = baseFlipIntervalInFrames,
            FlipsAtRandomInterval = flipsAtRandomInterval
        };
        SetActionBehavior(behavior, kind);
        return behavior;
    }

    public WanderActionBehavior SetWanderBehavior(
        ActionBehaviorKind kind,
        WanderBehaviorPattern pattern,
        float maxFramesIntervalBeforeMovingAgain,
        float radiusToWanderFromStartingPosition,
        float maxDistanceFromStartingPositionBeforeTeleported)
    {
        WanderActionBehavior behavior = new(this, kind)
        {
            WanderPattern = pattern,
            MaxFramesIntervalBeforeMovingAgain = maxFramesIntervalBeforeMovingAgain,
            RadiusToWanderFromStartingPosition = radiusToWanderFromStartingPosition,
            MaxDistanceFromStartingPositionBeforeTeleported = maxDistanceFromStartingPositionBeforeTeleported
        };
        SetActionBehavior(behavior, kind);
        return behavior;
    }

    // NOTE: The animstate AND the wander frames delay are the same so they conflict, but there's not much that can be done to address this
    public WanderWhenAnimstateIsWalkOrIdleActionBehavior SetWanderWhenAnimstateIsWalkOrIdleBehavior(
        ActionBehaviorKind kind,
        int animstateOverrideWhenNotChase,
        float radiusToWanderFromStartingPosition,
        float maxDistanceFromStartingPositionBeforeTeleported)
    {
        WanderWhenAnimstateIsWalkOrIdleActionBehavior behavior = new(this, kind)
        {
            AnimstateOverrideWhenNotChase = animstateOverrideWhenNotChase,
            RadiusToWanderFromStartingPosition = radiusToWanderFromStartingPosition,
            MaxDistanceFromStartingPositionBeforeTeleported = maxDistanceFromStartingPositionBeforeTeleported
        };
        SetActionBehavior(behavior, kind);
        return behavior;
    }

    public DisguiseActionBehavior SetDisguiseBehavior(ActionBehaviorKind kind)
    {
        DisguiseActionBehavior behavior = new(this, kind);
        SetActionBehavior(behavior, kind);
        return behavior;
    }

    public DisguiseOnceBeforeWanderActionBehavior SetDisguiseOnceBeforeWanderBehavior(
        ActionBehaviorKind kind,
        float maxFramesIntervalBeforeMovingAgain,
        float radiusToWanderFromStartingPosition,
        float maxDistanceFromStartingPositionBeforeTeleported)
    {
        DisguiseOnceBeforeWanderActionBehavior behavior = new(this, kind)
        {
            MaxFramesIntervalBeforeMovingAgain = maxFramesIntervalBeforeMovingAgain,
            RadiusToWanderFromStartingPosition = radiusToWanderFromStartingPosition,
            MaxDistanceFromStartingPositionBeforeTeleported = maxDistanceFromStartingPositionBeforeTeleported
        };
        SetActionBehavior(behavior, kind);
        return behavior;
    }

    // TODO: Consider handling items drops not being allowed with this or patch the game to allow them
    public MoveAlongPathActionBehavior SetMoveAlongPathBehavior(
        ActionBehaviorKind kind,
        float delayFramesBeforeMovingToNextNode,
        bool jumpWhileMoving,
        ICollection<Vector3> positionNodesInPath)
    {
        MoveAlongPathActionBehavior behavior = new(this, kind)
        {
            DelayFramesBeforeMovingToNextNode = delayFramesBeforeMovingToNextNode,
            JumpWhileMoving = jumpWhileMoving,
        };
        behavior.ChangeMovementPathNodePositions(positionNodesInPath);
        SetActionBehavior(behavior, kind);
        return behavior;
    }

    public ChargeAtPlayerActionBehavior SetChargeAtPlayerBehavior(
        ActionBehaviorKind kind,
        bool lockSpriteFlipDuringCharge)
    {
        ChargeAtPlayerActionBehavior behavior = new(this, kind)
        {
            LockSpriteFlipDuringCharge = lockSpriteFlipDuringCharge
        };
        SetActionBehavior(behavior, kind);
        return behavior;
    }

    public ShootProjectileActionBehavior SetShootProjectileBehavior(ActionBehaviorKind kind)
    {
        ShootProjectileActionBehavior behavior = new(this, kind);
        SetActionBehavior(behavior, kind);
        return behavior;
    }

    public UnmovableWhenDizzyActionBehavior SetUnmovableWhenDizzyBehavior(ActionBehaviorKind kind)
    {
        UnmovableWhenDizzyActionBehavior behavior = new(this, kind);
        SetActionBehavior(behavior, kind);
        return behavior;
    }

    public ChangeAnimstateInRadiusActionBehavior SetChangeAnimstateInRadiusGlobalBehavior(
        float radius,
        int animstateWhenOutsideRadius,
        int animstateWhenInsideRadius)
    {
        ChangeAnimstateInRadiusActionBehavior behavior = new(this)
        {
            Radius = radius,
            AnimstateWhenOutsideRadius = animstateWhenOutsideRadius,
            AnimstateWhenInsideRadius = animstateWhenInsideRadius
        };
        SetActionBehavior(behavior, null);
        return behavior;
    }

    public void ClearBehavior(ActionBehaviorKind kind)
    {
        SetActionBehavior(null, kind);
    }

    private void SetActionBehavior(ActionBehavior? behavior, ActionBehaviorKind? kind)
    {
        switch (kind)
        {
            case ActionBehaviorKind.OutOfRange:
                OutOfBehaviorRangeBehavior = behavior;
                if (behavior is null)
                {
                    InternalPrimaryBehavior = NPCControl.ActionBehaviors.None;
                    InternalPrimaryActionFrequency = 0f;
                }

                if (InBehaviorRangeBehavior is ChasePlayerWhenAnimstateIsChaseActionBehavior)
                {
                    InBehaviorRangeBehavior = null;
                    InternalSecondaryBehavior = NPCControl.ActionBehaviors.None;
                    InternalSecondaryActionFrequency = 0f;
                }
                break;
            case ActionBehaviorKind.InRange:
                InBehaviorRangeBehavior = behavior;
                if (behavior is null)
                {
                    InternalSecondaryBehavior = NPCControl.ActionBehaviors.None;
                    InternalSecondaryActionFrequency = 0f;
                }

                if (OutOfBehaviorRangeBehavior is ChasePlayerWhenAnimstateIsChaseActionBehavior)
                {
                    OutOfBehaviorRangeBehavior = null;
                    InternalPrimaryBehavior = NPCControl.ActionBehaviors.None;
                    InternalPrimaryActionFrequency = 0f;
                }
                break;
            case null:
                OutOfBehaviorRangeBehavior = behavior;
                InBehaviorRangeBehavior = behavior;
                break;
            default:
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(kind));
                break;
        }
    }
}