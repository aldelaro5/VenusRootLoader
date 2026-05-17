using CommunityToolkit.Diagnostics;
using System.Collections.ObjectModel;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Api.MapEntities.ActionBehaviors;
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

    public void SetFacingBehavior(ActionBehaviorKind kind, FacingBehaviorDirection direction)
    {
        NPCControl.ActionBehaviors behaviorType = direction switch
        {
            FacingBehaviorDirection.TowardsPlayer => NPCControl.ActionBehaviors.FacePlayer,
            FacingBehaviorDirection.AwayFromPlayer => NPCControl.ActionBehaviors.FaceAwayFromPlayer,
            FacingBehaviorDirection.TowardsEntityRightVector => NPCControl.ActionBehaviors.FaceAhead,
            FacingBehaviorDirection.TowardsEntityLeftVector => NPCControl.ActionBehaviors.FaceBehind,
            FacingBehaviorDirection.TowardsEntityForwardVector => NPCControl.ActionBehaviors.FaceUp,
            FacingBehaviorDirection.TowardsEntityBackwardVector => NPCControl.ActionBehaviors.FaceDown,
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<NPCControl.ActionBehaviors>(nameof(direction))
        };

        SetBehaviorTypeAndFrequency(kind, behaviorType, null);
    }

    public void SetChasePlayerBehavior(ActionBehaviorKind kind, bool chaseOnWater)
    {
        NPCControl.ActionBehaviors type = chaseOnWater
            ? NPCControl.ActionBehaviors.ChaseOnWater
            : NPCControl.ActionBehaviors.ChasePlayer;

        SetBehaviorTypeAndFrequency(kind, type, null);
    }

    public void SetChasePlayerBehaviorWhenAnimstateIsChase(ActionBehaviorKind kind, int animstateOverrideWhenNotChase)
    {
        SetBehaviorTypeAndFrequency(kind, NPCControl.ActionBehaviors.ChaseWhenAnim, animstateOverrideWhenNotChase);
    }

    public void SetChaseAndAttackPlayerBehavior(
        ActionBehaviorKind kind,
        float minimumDistanceFromPlayerBeforeAttacking,
        bool attacksFromUnderground)
    {
        NPCControl.ActionBehaviors type = attacksFromUnderground
            ? NPCControl.ActionBehaviors.ChargeAttackUnderground
            : NPCControl.ActionBehaviors.ChargeAndAttack;

        SetBehaviorTypeAndFrequency(kind, type, minimumDistanceFromPlayerBeforeAttacking);
    }

    public void SetFleeFromPlayerBehavior(ActionBehaviorKind kind)
    {
        SetBehaviorTypeAndFrequency(kind, NPCControl.ActionBehaviors.FleeFromPlayer, null);
    }

    public void SetSpriteFlipBehavior(
        ActionBehaviorKind kind,
        float baseFlipIntervalInFrames,
        bool flipsAtRandomInterval)
    {
        NPCControl.ActionBehaviors type = flipsAtRandomInterval
            ? NPCControl.ActionBehaviors.TurnRandomly
            : NPCControl.ActionBehaviors.TurnFixedInterval;

        SetBehaviorTypeAndFrequency(kind, type, baseFlipIntervalInFrames);
    }

    public void SetWanderBehavior(
        ActionBehaviorKind kind,
        WanderBehaviorPattern pattern,
        float maxFramesIntervalBeforeMovingAgain,
        float radiusToWanderFromStartingPosition,
        float maxDistanceFromStartingPositionBeforeTeleported)
    {
        NPCControl.ActionBehaviors type = pattern switch
        {
            WanderBehaviorPattern.Regular => NPCControl.ActionBehaviors.Wander,
            WanderBehaviorPattern.FromUnderground => NPCControl.ActionBehaviors.WanderUnderground,
            WanderBehaviorPattern.CanWonderWhenInactive => NPCControl.ActionBehaviors.WanderOffscreen,
            WanderBehaviorPattern.WillNotWarpIfNoWanderPositionIsAvailable => NPCControl.ActionBehaviors.WanderNoWarp,
            WanderBehaviorPattern.OnWater => NPCControl.ActionBehaviors.WanderOnWater,
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<NPCControl.ActionBehaviors>(nameof(pattern))
        };

        SetBehaviorTypeAndFrequency(kind, type, maxFramesIntervalBeforeMovingAgain);
        InternalWanderRadius = radiusToWanderFromStartingPosition;
        InternalTeleportRadius = maxDistanceFromStartingPositionBeforeTeleported;
    }

    // NOTE: The animstate AND the wander frames delay are the same so they conflict, but there's not much that can be done to address this
    public void SetWanderBehaviorWhenAnimstateIsWalkOrIdle(
        ActionBehaviorKind kind,
        int animstateOverrideWhenNotChase,
        float radiusToWanderFromStartingPosition,
        float maxDistanceFromStartingPositionBeforeTeleported)
    {
        SetBehaviorTypeAndFrequency(kind, NPCControl.ActionBehaviors.WalkWhenAnim, animstateOverrideWhenNotChase);
        InternalWanderRadius = radiusToWanderFromStartingPosition;
        InternalTeleportRadius = maxDistanceFromStartingPositionBeforeTeleported;
    }

    public void SetDisguiseBehavior(ActionBehaviorKind kind)
    {
        SetBehaviorTypeAndFrequency(kind, NPCControl.ActionBehaviors.Disguise, null);
    }

    public void SetDisguiseOnceBeforeWanderBehavior(
        ActionBehaviorKind kind,
        float maxFramesIntervalBeforeMovingAgain,
        float radiusToWanderFromStartingPosition,
        float maxDistanceFromStartingPositionBeforeTeleported)
    {
        SetBehaviorTypeAndFrequency(kind, NPCControl.ActionBehaviors.DisguiseOnce, maxFramesIntervalBeforeMovingAgain);
        InternalWanderRadius = radiusToWanderFromStartingPosition;
        InternalTeleportRadius = maxDistanceFromStartingPositionBeforeTeleported;
    }

    public void SetPathBehavior(
        ActionBehaviorKind kind,
        float delayFramesBeforeMovingToNextNode,
        bool jumpWhileMoving,
        ICollection<Vector3> positionNodesInPath)
    {
        // TODO: Change the throw so it applies to all enemies except the ones without item drops
        if (InternalVectorData.Count > 0)
        {
            ThrowHelper.ThrowInvalidOperationException(
                "It is not possible to set a path behavior when there are item drops");
        }

        NPCControl.ActionBehaviors type = jumpWhileMoving
            ? NPCControl.ActionBehaviors.SetPathJump
            : NPCControl.ActionBehaviors.SetPath;

        SetBehaviorTypeAndFrequency(kind, type, delayFramesBeforeMovingToNextNode);
        InternalVectorData.AddRange(positionNodesInPath);
    }

    public void SetChargeAtPlayerBehavior(ActionBehaviorKind kind, bool lockSpriteFlipDuringCharge)
    {
        NPCControl.ActionBehaviors type = lockSpriteFlipDuringCharge
            ? NPCControl.ActionBehaviors.ChargeAtPlayerFlipSprite
            : NPCControl.ActionBehaviors.ChargeAtPlayer;

        SetBehaviorTypeAndFrequency(kind, type, null);
    }

    public void SetShootProjectileBehavior(ActionBehaviorKind kind)
    {
        SetBehaviorTypeAndFrequency(kind, NPCControl.ActionBehaviors.ShootProjectile, null);
    }

    public void SetUnmovableWhenDizzyBehavior(ActionBehaviorKind kind)
    {
        SetBehaviorTypeAndFrequency(kind, NPCControl.ActionBehaviors.Unmoveable, null);
    }

    public void SetChangeAnimstateInRadiusGlobalBehavior(
        float radius,
        int animstateWhenOutsideRadius,
        int animstateWhenInsideRadius)
    {
        InternalPrimaryBehavior = NPCControl.ActionBehaviors.ChangeSpriteInRandius;
        InternalSecondaryBehavior = NPCControl.ActionBehaviors.ChangeSpriteInRandius;
        InternalWanderRadius = radius;
        InternalPrimaryActionFrequency = animstateWhenOutsideRadius;
        InternalSecondaryActionFrequency = animstateWhenInsideRadius;
    }

    public void ClearBehavior(ActionBehaviorKind kind)
    {
        SetBehaviorTypeAndFrequency(kind, NPCControl.ActionBehaviors.None, 0f);
    }

    private void SetBehaviorTypeAndFrequency(
        ActionBehaviorKind kind,
        NPCControl.ActionBehaviors behaviorType,
        float? frequency)
    {
        switch (kind)
        {
            case ActionBehaviorKind.OutOfRange:
                InternalPrimaryBehavior = behaviorType;
                if (frequency.HasValue)
                    InternalPrimaryActionFrequency = frequency.Value;
                break;
            case ActionBehaviorKind.InRange:
                InternalSecondaryBehavior = behaviorType;
                if (frequency.HasValue)
                    InternalSecondaryActionFrequency = frequency.Value;
                break;
            default:
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(kind));
                break;
        }
    }
}