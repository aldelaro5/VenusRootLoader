using CommunityToolkit.Diagnostics;
using UnityEngine;
using VenusRootLoader.Api.Leaves.MapEntities.ActionBehaviors.Enums;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.ActionBehaviors;

public sealed class MapEntityBehaviors
{
    private readonly MapEntityLeaf _mapEntityLeaf;

    public ActionBehavior? OutOfBehaviorRangeBehavior { get; private set; }
    public ActionBehavior? InBehaviorRangeBehavior { get; private set; }

    internal MapEntityBehaviors(MapEntityLeaf entityLeaf) { _mapEntityLeaf = entityLeaf; }

    internal void InitializeBehaviorFromExisting(IRegistryResolver registryResolver)
    {
        InitializeBehaviorFromExisting(ActionBehaviorKind.OutOfRange, registryResolver);
        InitializeBehaviorFromExisting(ActionBehaviorKind.InRange, registryResolver);
    }

    private void InitializeBehaviorFromExisting(ActionBehaviorKind kind, IRegistryResolver registryResolver)
    {
        NPCControl.ActionBehaviors internalType = kind switch
        {
            ActionBehaviorKind.OutOfRange => _mapEntityLeaf.InternalOutOfRangeBehavior,
            ActionBehaviorKind.InRange => _mapEntityLeaf.InternalInRangeBehavior,
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
            NPCControl.ActionBehaviors.FacePlayer => new FaceDirectionActionBehavior(_mapEntityLeaf, kind),
            NPCControl.ActionBehaviors.ChasePlayer => new ChasePlayerActionBehavior(_mapEntityLeaf, kind),
            NPCControl.ActionBehaviors.FleeFromPlayer => new FleeFromPlayerActionBehavior(_mapEntityLeaf, kind),
            NPCControl.ActionBehaviors.TurnRandomly => new SpriteFlipActionBehavior(_mapEntityLeaf, kind),
            NPCControl.ActionBehaviors.Wander => new WanderActionBehavior(_mapEntityLeaf, kind),
            NPCControl.ActionBehaviors.FaceAwayFromPlayer => new FaceDirectionActionBehavior(_mapEntityLeaf, kind),
            NPCControl.ActionBehaviors.TurnFixedInterval => new SpriteFlipActionBehavior(_mapEntityLeaf, kind),
            NPCControl.ActionBehaviors.Disguise => new DisguiseActionBehavior(_mapEntityLeaf, kind),
            NPCControl.ActionBehaviors.DisguiseOnce => new DisguiseOnceBeforeWanderActionBehavior(_mapEntityLeaf, kind),
            NPCControl.ActionBehaviors.FaceAhead => new FaceDirectionActionBehavior(_mapEntityLeaf, kind),
            NPCControl.ActionBehaviors.FaceBehind => new FaceDirectionActionBehavior(_mapEntityLeaf, kind),
            NPCControl.ActionBehaviors.FaceUp => new FaceDirectionActionBehavior(_mapEntityLeaf, kind),
            NPCControl.ActionBehaviors.FaceDown => new FaceDirectionActionBehavior(_mapEntityLeaf, kind),
            NPCControl.ActionBehaviors.SetPath => new MoveAlongPathActionBehavior(_mapEntityLeaf, kind),
            NPCControl.ActionBehaviors.ChargeAtPlayer => new ChargeAtPlayerActionBehavior(_mapEntityLeaf, kind),
            NPCControl.ActionBehaviors.ChargeAtPlayerFlipSprite => new ChargeAtPlayerActionBehavior(
                _mapEntityLeaf,
                kind),
            NPCControl.ActionBehaviors.ShootProjectile => new ShootProjectileActionBehavior(_mapEntityLeaf, kind),
            NPCControl.ActionBehaviors.ChargeAndAttack => new ChaseAndAttackPlayerActionBehavior(_mapEntityLeaf, kind),
            NPCControl.ActionBehaviors.Unmoveable => new UnmovableWhenDizzyActionBehavior(_mapEntityLeaf, kind),
            NPCControl.ActionBehaviors.ChargeAttackUnderground => new ChaseAndAttackPlayerActionBehavior(
                _mapEntityLeaf,
                kind),
            NPCControl.ActionBehaviors.WanderUnderground => new WanderActionBehavior(_mapEntityLeaf, kind),
            NPCControl.ActionBehaviors.StealthAI => CreateNewStealthSpotBehaviorFromExisting(registryResolver),
            NPCControl.ActionBehaviors.SetPathJump => new MoveAlongPathActionBehavior(_mapEntityLeaf, kind),
            NPCControl.ActionBehaviors.ChangeSpriteInRandius => new ChangeAnimstateInRadiusActionBehavior(
                _mapEntityLeaf),
            NPCControl.ActionBehaviors.ChaseWhenAnim => new ChasePlayerWhenAnimstateIsChaseActionBehavior(
                _mapEntityLeaf,
                kind),
            NPCControl.ActionBehaviors.WalkWhenAnim => new WanderWhenAnimstateIsWalkOrIdleActionBehavior(
                _mapEntityLeaf,
                kind),
            NPCControl.ActionBehaviors.WanderOffscreen => new WanderActionBehavior(_mapEntityLeaf, kind),
            NPCControl.ActionBehaviors.WanderNoWarp => new WanderActionBehavior(_mapEntityLeaf, kind),
            NPCControl.ActionBehaviors.WanderOnWater => new WanderActionBehavior(_mapEntityLeaf, kind),
            NPCControl.ActionBehaviors.ChaseOnWater => new ChasePlayerActionBehavior(_mapEntityLeaf, kind),
            _ => ThrowHelper.ThrowInvalidOperationException<ActionBehavior?>(
                $"The internal action behavior type {internalType} is not supported.")
        };
    }

    private StealthSpotBehavior CreateNewStealthSpotBehaviorFromExisting(IRegistryResolver registryResolver)
    {
        StealthSpotBehavior behavior = new(_mapEntityLeaf);
        behavior.InitializeFromExisting(registryResolver);
        return behavior;
    }

    public FaceDirectionActionBehavior SetFaceDirectionBehavior(
        ActionBehaviorKind kind,
        FacingBehaviorDirection direction)
    {
        FaceDirectionActionBehavior behavior = new(_mapEntityLeaf, kind) { FacingDirection = direction };
        SetActionBehavior(behavior, kind);
        return behavior;
    }

    public ChasePlayerActionBehavior SetChasePlayerBehavior(
        ActionBehaviorKind kind,
        bool chaseOnWater,
        float movementSpeedMultiplier)
    {
        ChasePlayerActionBehavior behavior = new(_mapEntityLeaf, kind)
        {
            ChaseOnWater = chaseOnWater,
            MovementSpeedMultiplier = movementSpeedMultiplier
        };
        SetActionBehavior(behavior, kind);
        return behavior;
    }

    public ChasePlayerWhenAnimstateIsChaseActionBehavior SetChasePlayerWhenAnimstateIsChaseBehavior(
        ActionBehaviorKind kind,
        int animstateOverrideWhenNotChase,
        float movementSpeedMultiplier)
    {
        ChasePlayerWhenAnimstateIsChaseActionBehavior behavior = new(_mapEntityLeaf, kind)
        {
            AnimstateOverrideWhenNotChase = animstateOverrideWhenNotChase,
            MovementSpeedMultiplier = movementSpeedMultiplier
        };
        SetActionBehavior(behavior, kind);
        return behavior;
    }

    public ChaseAndAttackPlayerActionBehavior SetChaseAndAttackPlayerBehavior(
        ActionBehaviorKind kind,
        float minimumDistanceFromPlayerBeforeAttacking,
        bool attacksFromUnderground,
        float movementSpeedMultiplier)
    {
        ChaseAndAttackPlayerActionBehavior behavior = new(_mapEntityLeaf, kind)
        {
            MinimumDistanceFromPlayerBeforeAttacking = minimumDistanceFromPlayerBeforeAttacking,
            AttacksFromUnderground = attacksFromUnderground,
            MovementSpeedMultiplier = movementSpeedMultiplier
        };
        SetActionBehavior(behavior, kind);
        return behavior;
    }

    public FleeFromPlayerActionBehavior SetFleeFromPlayerBehavior(ActionBehaviorKind kind)
    {
        FleeFromPlayerActionBehavior behavior = new(_mapEntityLeaf, kind);
        SetActionBehavior(behavior, kind);
        return behavior;
    }

    public SpriteFlipActionBehavior SetSpriteFlipBehavior(
        ActionBehaviorKind kind,
        float baseFlipIntervalInFrames,
        bool flipsAtRandomInterval)
    {
        SpriteFlipActionBehavior behavior = new(_mapEntityLeaf, kind)
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
        WanderActionBehavior behavior = new(_mapEntityLeaf, kind)
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
        WanderWhenAnimstateIsWalkOrIdleActionBehavior behavior = new(_mapEntityLeaf, kind)
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
        DisguiseActionBehavior behavior = new(_mapEntityLeaf, kind);
        SetActionBehavior(behavior, kind);
        return behavior;
    }

    public DisguiseOnceBeforeWanderActionBehavior SetDisguiseOnceBeforeWanderBehavior(
        ActionBehaviorKind kind,
        float maxFramesIntervalBeforeMovingAgain,
        float radiusToWanderFromStartingPosition,
        float maxDistanceFromStartingPositionBeforeTeleported)
    {
        DisguiseOnceBeforeWanderActionBehavior behavior = new(_mapEntityLeaf, kind)
        {
            MaxFramesIntervalBeforeMovingAgain = maxFramesIntervalBeforeMovingAgain,
            RadiusToWanderFromStartingPosition = radiusToWanderFromStartingPosition,
            MaxDistanceFromStartingPositionBeforeTeleported = maxDistanceFromStartingPositionBeforeTeleported
        };
        SetActionBehavior(behavior, kind);
        return behavior;
    }

    public MoveAlongPathActionBehavior SetMoveAlongPathBehavior(
        ActionBehaviorKind kind,
        float delayFramesBeforeMovingToNextNode,
        bool jumpWhileMoving,
        ICollection<Vector3> positionNodesInPath)
    {
        MoveAlongPathActionBehavior behavior = new(_mapEntityLeaf, kind)
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
        bool lockSpriteFlipDuringCharge,
        float movementSpeedMultiplier)
    {
        ChargeAtPlayerActionBehavior behavior = new(_mapEntityLeaf, kind)
        {
            LockSpriteFlipDuringCharge = lockSpriteFlipDuringCharge,
            MovementSpeedMultiplier = movementSpeedMultiplier
        };
        SetActionBehavior(behavior, kind);
        return behavior;
    }

    public ShootProjectileActionBehavior SetShootProjectileBehavior(ActionBehaviorKind kind)
    {
        ShootProjectileActionBehavior behavior = new(_mapEntityLeaf, kind);
        SetActionBehavior(behavior, kind);
        return behavior;
    }

    public UnmovableWhenDizzyActionBehavior SetUnmovableWhenDizzyBehavior(ActionBehaviorKind kind)
    {
        UnmovableWhenDizzyActionBehavior behavior = new(_mapEntityLeaf, kind);
        SetActionBehavior(behavior, kind);
        return behavior;
    }

    public ChangeAnimstateInRadiusActionBehavior SetChangeAnimstateInRadiusGlobalBehavior(
        float radius,
        int animstateWhenOutsideRadius,
        int animstateWhenInsideRadius)
    {
        ChangeAnimstateInRadiusActionBehavior behavior = new(_mapEntityLeaf)
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
        StealthSpotBehavior behavior = new(_mapEntityLeaf)
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
                    _mapEntityLeaf.InternalOutOfRangeBehavior = NPCControl.ActionBehaviors.None;
                    _mapEntityLeaf.InternalOutOfRangeActionFrequency = 0f;
                }

                if (InBehaviorRangeBehavior is ChasePlayerWhenAnimstateIsChaseActionBehavior)
                {
                    InBehaviorRangeBehavior = null;
                    _mapEntityLeaf.InternalInRangeBehavior = NPCControl.ActionBehaviors.None;
                    _mapEntityLeaf.InternalInRangeActionFrequency = 0f;
                }

                break;
            case ActionBehaviorKind.InRange:
                InBehaviorRangeBehavior = behavior;
                if (behavior is null)
                {
                    _mapEntityLeaf.InternalInRangeBehavior = NPCControl.ActionBehaviors.None;
                    _mapEntityLeaf.InternalInRangeActionFrequency = 0f;
                }

                if (OutOfBehaviorRangeBehavior is ChasePlayerWhenAnimstateIsChaseActionBehavior)
                {
                    OutOfBehaviorRangeBehavior = null;
                    _mapEntityLeaf.InternalOutOfRangeBehavior = NPCControl.ActionBehaviors.None;
                    _mapEntityLeaf.InternalOutOfRangeActionFrequency = 0f;
                }

                if (OutOfBehaviorRangeBehavior is StealthSpotBehavior)
                {
                    OutOfBehaviorRangeBehavior = null;
                    _mapEntityLeaf.InternalOutOfRangeBehavior = NPCControl.ActionBehaviors.None;
                    _mapEntityLeaf.InternalOutOfRangeActionFrequency = 0f;
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