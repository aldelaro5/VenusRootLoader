using CommunityToolkit.Diagnostics;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Api.MapEntities.ActionBehaviors.Enums;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.MapEntities.ActionBehaviors;

public sealed class MapEntityBehaviors
{
    private readonly MapEntity _mapEntity;

    public ActionBehavior? OutOfBehaviorRangeBehavior { get; private set; }
    public ActionBehavior? InBehaviorRangeBehavior { get; private set; }

    public float BehaviorRangeRadius
    {
        get => _mapEntity.InternalRadius;
        set => _mapEntity.InternalRadius = value;
    }

    internal MapEntityBehaviors(MapEntity entity) { _mapEntity = entity; }

    internal void InitializeBehaviorFromExisting(IRegistryResolver registryResolver)
    {
        InitializeBehaviorFromExisting(ActionBehaviorKind.OutOfRange, registryResolver);
        InitializeBehaviorFromExisting(ActionBehaviorKind.InRange, registryResolver);
    }

    private void InitializeBehaviorFromExisting(ActionBehaviorKind kind, IRegistryResolver registryResolver)
    {
        NPCControl.ActionBehaviors internalType = kind switch
        {
            ActionBehaviorKind.OutOfRange => _mapEntity.InternalPrimaryBehavior,
            ActionBehaviorKind.InRange => _mapEntity.InternalSecondaryBehavior,
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<NPCControl.ActionBehaviors>(nameof(kind))
        };

        ActionBehavior? behavior = MapExistingInternalBehaviorType(kind, internalType, registryResolver);

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
        NPCControl.ActionBehaviors internalType,
        IRegistryResolver registryResolver)
    {
        return internalType switch
        {
            NPCControl.ActionBehaviors.None => null,
            NPCControl.ActionBehaviors.FacePlayer => new FaceDirectionActionBehavior(_mapEntity, kind),
            NPCControl.ActionBehaviors.ChasePlayer => new ChasePlayerActionBehavior(_mapEntity, kind),
            NPCControl.ActionBehaviors.FleeFromPlayer => new FleeFromPlayerActionBehavior(_mapEntity, kind),
            NPCControl.ActionBehaviors.TurnRandomly => new SpriteFlipActionBehavior(_mapEntity, kind),
            NPCControl.ActionBehaviors.Wander => new WanderActionBehavior(_mapEntity, kind),
            NPCControl.ActionBehaviors.FaceAwayFromPlayer => new FaceDirectionActionBehavior(_mapEntity, kind),
            NPCControl.ActionBehaviors.TurnFixedInterval => new SpriteFlipActionBehavior(_mapEntity, kind),
            NPCControl.ActionBehaviors.Disguise => new DisguiseActionBehavior(_mapEntity, kind),
            NPCControl.ActionBehaviors.DisguiseOnce => new DisguiseOnceBeforeWanderActionBehavior(_mapEntity, kind),
            NPCControl.ActionBehaviors.FaceAhead => new FaceDirectionActionBehavior(_mapEntity, kind),
            NPCControl.ActionBehaviors.FaceBehind => new FaceDirectionActionBehavior(_mapEntity, kind),
            NPCControl.ActionBehaviors.FaceUp => new FaceDirectionActionBehavior(_mapEntity, kind),
            NPCControl.ActionBehaviors.FaceDown => new FaceDirectionActionBehavior(_mapEntity, kind),
            NPCControl.ActionBehaviors.SetPath => new MoveAlongPathActionBehavior(_mapEntity, kind),
            NPCControl.ActionBehaviors.ChargeAtPlayer => new ChargeAtPlayerActionBehavior(_mapEntity, kind),
            NPCControl.ActionBehaviors.ChargeAtPlayerFlipSprite => new ChargeAtPlayerActionBehavior(_mapEntity, kind),
            NPCControl.ActionBehaviors.ShootProjectile => new ShootProjectileActionBehavior(_mapEntity, kind),
            NPCControl.ActionBehaviors.ChargeAndAttack => new ChaseAndAttackPlayerActionBehavior(_mapEntity, kind),
            NPCControl.ActionBehaviors.Unmoveable => new UnmovableWhenDizzyActionBehavior(_mapEntity, kind),
            NPCControl.ActionBehaviors.ChargeAttackUnderground => new ChaseAndAttackPlayerActionBehavior(
                _mapEntity,
                kind),
            NPCControl.ActionBehaviors.WanderUnderground => new WanderActionBehavior(_mapEntity, kind),
            NPCControl.ActionBehaviors.StealthAI => CreateNewStealthSpotBehaviorFromExisting(registryResolver),
            NPCControl.ActionBehaviors.SetPathJump => new MoveAlongPathActionBehavior(_mapEntity, kind),
            NPCControl.ActionBehaviors.ChangeSpriteInRandius => new ChangeAnimstateInRadiusActionBehavior(_mapEntity),
            NPCControl.ActionBehaviors.ChaseWhenAnim => new ChasePlayerWhenAnimstateIsChaseActionBehavior(
                _mapEntity,
                kind),
            NPCControl.ActionBehaviors.WalkWhenAnim => new WanderWhenAnimstateIsWalkOrIdleActionBehavior(
                _mapEntity,
                kind),
            NPCControl.ActionBehaviors.WanderOffscreen => new WanderActionBehavior(_mapEntity, kind),
            NPCControl.ActionBehaviors.WanderNoWarp => new WanderActionBehavior(_mapEntity, kind),
            NPCControl.ActionBehaviors.WanderOnWater => new WanderActionBehavior(_mapEntity, kind),
            NPCControl.ActionBehaviors.ChaseOnWater => new ChasePlayerActionBehavior(_mapEntity, kind),
            _ => ThrowHelper.ThrowInvalidOperationException<ActionBehavior?>(
                $"The internal action behavior type {internalType} is not supported.")
        };
    }

    private StealthSpotBehavior CreateNewStealthSpotBehaviorFromExisting(IRegistryResolver registryResolver)
    {
        StealthSpotBehavior behavior = new(_mapEntity);
        behavior.InitializeFromExisting(registryResolver);
        return behavior;
    }

    public FaceDirectionActionBehavior SetFaceDirectionBehavior(
        ActionBehaviorKind kind,
        FacingBehaviorDirection direction)
    {
        FaceDirectionActionBehavior behavior = new(_mapEntity, kind) { FacingDirection = direction };
        SetActionBehavior(behavior, kind);
        return behavior;
    }

    public ChasePlayerActionBehavior SetChasePlayerBehavior(ActionBehaviorKind kind, bool chaseOnWater)
    {
        ChasePlayerActionBehavior behavior = new(_mapEntity, kind) { ChaseOnWater = chaseOnWater };
        SetActionBehavior(behavior, kind);
        return behavior;
    }

    public ChasePlayerWhenAnimstateIsChaseActionBehavior SetChasePlayerWhenAnimstateIsChaseBehavior(
        ActionBehaviorKind kind,
        int animstateOverrideWhenNotChase)
    {
        ChasePlayerWhenAnimstateIsChaseActionBehavior behavior = new(_mapEntity, kind)
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
        ChaseAndAttackPlayerActionBehavior behavior = new(_mapEntity, kind)
        {
            MinimumDistanceFromPlayerBeforeAttacking = minimumDistanceFromPlayerBeforeAttacking,
            AttacksFromUnderground = attacksFromUnderground
        };
        SetActionBehavior(behavior, kind);
        return behavior;
    }

    public FleeFromPlayerActionBehavior SetFleeFromPlayerBehavior(ActionBehaviorKind kind)
    {
        FleeFromPlayerActionBehavior behavior = new(_mapEntity, kind);
        SetActionBehavior(behavior, kind);
        return behavior;
    }

    public SpriteFlipActionBehavior SetSpriteFlipBehavior(
        ActionBehaviorKind kind,
        float baseFlipIntervalInFrames,
        bool flipsAtRandomInterval)
    {
        SpriteFlipActionBehavior behavior = new(_mapEntity, kind)
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
        WanderActionBehavior behavior = new(_mapEntity, kind)
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
        WanderWhenAnimstateIsWalkOrIdleActionBehavior behavior = new(_mapEntity, kind)
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
        DisguiseActionBehavior behavior = new(_mapEntity, kind);
        SetActionBehavior(behavior, kind);
        return behavior;
    }

    public DisguiseOnceBeforeWanderActionBehavior SetDisguiseOnceBeforeWanderBehavior(
        ActionBehaviorKind kind,
        float maxFramesIntervalBeforeMovingAgain,
        float radiusToWanderFromStartingPosition,
        float maxDistanceFromStartingPositionBeforeTeleported)
    {
        DisguiseOnceBeforeWanderActionBehavior behavior = new(_mapEntity, kind)
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
        MoveAlongPathActionBehavior behavior = new(_mapEntity, kind)
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
        ChargeAtPlayerActionBehavior behavior = new(_mapEntity, kind)
        {
            LockSpriteFlipDuringCharge = lockSpriteFlipDuringCharge
        };
        SetActionBehavior(behavior, kind);
        return behavior;
    }

    public ShootProjectileActionBehavior SetShootProjectileBehavior(ActionBehaviorKind kind)
    {
        ShootProjectileActionBehavior behavior = new(_mapEntity, kind);
        SetActionBehavior(behavior, kind);
        return behavior;
    }

    public UnmovableWhenDizzyActionBehavior SetUnmovableWhenDizzyBehavior(ActionBehaviorKind kind)
    {
        UnmovableWhenDizzyActionBehavior behavior = new(_mapEntity, kind);
        SetActionBehavior(behavior, kind);
        return behavior;
    }

    public ChangeAnimstateInRadiusActionBehavior SetChangeAnimstateInRadiusGlobalBehavior(
        float radius,
        int animstateWhenOutsideRadius,
        int animstateWhenInsideRadius)
    {
        ChangeAnimstateInRadiusActionBehavior behavior = new(_mapEntity)
        {
            Radius = radius,
            AnimstateWhenOutsideRadius = animstateWhenOutsideRadius,
            AnimstateWhenInsideRadius = animstateWhenInsideRadius
        };
        SetActionBehavior(behavior, null);
        return behavior;
    }

    public StealthSpotBehavior SetStealthSpotGlobalBehavior(
        Branch<EventLeaf>? eventToStartWhenSpottingPlayer,
        float? delayFramesBeforeMovingToNextNode,
        int visionLengthInUnits)
    {
        StealthSpotBehavior behavior = new(_mapEntity)
        {
            EventToStartWhenSpottingPlayer = eventToStartWhenSpottingPlayer,
            VisionLengthInUnits = visionLengthInUnits,
            DelayFramesBeforeMovingToNextNode = delayFramesBeforeMovingToNextNode
        };
        SetActionBehavior(null, ActionBehaviorKind.InRange);
        SetActionBehavior(behavior, ActionBehaviorKind.OutOfRange);
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
                    _mapEntity.InternalPrimaryBehavior = NPCControl.ActionBehaviors.None;
                    _mapEntity.InternalPrimaryActionFrequency = 0f;
                }

                if (InBehaviorRangeBehavior is ChasePlayerWhenAnimstateIsChaseActionBehavior)
                {
                    InBehaviorRangeBehavior = null;
                    _mapEntity.InternalSecondaryBehavior = NPCControl.ActionBehaviors.None;
                    _mapEntity.InternalSecondaryActionFrequency = 0f;
                }

                break;
            case ActionBehaviorKind.InRange:
                InBehaviorRangeBehavior = behavior;
                if (behavior is null)
                {
                    _mapEntity.InternalSecondaryBehavior = NPCControl.ActionBehaviors.None;
                    _mapEntity.InternalSecondaryActionFrequency = 0f;
                }

                if (OutOfBehaviorRangeBehavior is ChasePlayerWhenAnimstateIsChaseActionBehavior)
                {
                    OutOfBehaviorRangeBehavior = null;
                    _mapEntity.InternalPrimaryBehavior = NPCControl.ActionBehaviors.None;
                    _mapEntity.InternalPrimaryActionFrequency = 0f;
                }

                if (OutOfBehaviorRangeBehavior is StealthSpotBehavior)
                {
                    OutOfBehaviorRangeBehavior = null;
                    _mapEntity.InternalPrimaryBehavior = NPCControl.ActionBehaviors.None;
                    _mapEntity.InternalPrimaryActionFrequency = 0f;
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