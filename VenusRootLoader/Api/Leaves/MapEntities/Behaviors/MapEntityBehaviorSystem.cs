using CommunityToolkit.Diagnostics;
using UnityEngine;
using VenusRootLoader.Api.Leaves.MapEntities.Behaviors.Enums;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Behaviors;

public sealed class MapEntityBehaviorSystem
{
    private readonly MapEntityLeaf _mapEntityLeaf;

    public MapEntityBehavior? OutOfBehaviorRangeBehavior { get; private set; }
    public MapEntityBehavior? InBehaviorRangeBehavior { get; private set; }

    internal MapEntityBehaviorSystem(MapEntityLeaf entityLeaf) { _mapEntityLeaf = entityLeaf; }

    internal void InitializeBehaviorFromExisting(IRegistryResolver registryResolver)
    {
        InitializeBehaviorFromExisting(BehaviorKind.OutOfRange, registryResolver);
        InitializeBehaviorFromExisting(BehaviorKind.InRange, registryResolver);
    }

    private void InitializeBehaviorFromExisting(BehaviorKind kind, IRegistryResolver registryResolver)
    {
        NPCControl.ActionBehaviors internalType = kind switch
        {
            BehaviorKind.OutOfRange => _mapEntityLeaf.InternalOutOfRangeBehavior,
            BehaviorKind.InRange => _mapEntityLeaf.InternalInRangeBehavior,
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<NPCControl.ActionBehaviors>(nameof(kind))
        };

        MapEntityBehavior? behavior = MapExistingInternalBehaviorType(kind, internalType, registryResolver);

        switch (kind)
        {
            case BehaviorKind.OutOfRange:
                OutOfBehaviorRangeBehavior = behavior;
                break;
            case BehaviorKind.InRange:
                InBehaviorRangeBehavior = behavior;
                break;
            default:
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(kind));
                break;
        }
    }

    private MapEntityBehavior? MapExistingInternalBehaviorType(
        BehaviorKind kind,
        NPCControl.ActionBehaviors internalType,
        IRegistryResolver registryResolver)
    {
        return internalType switch
        {
            NPCControl.ActionBehaviors.None => null,
            NPCControl.ActionBehaviors.FacePlayer => new FaceDirectionMapEntityBehavior(_mapEntityLeaf, kind),
            NPCControl.ActionBehaviors.ChasePlayer => new ChasePlayerMapEntityBehavior(_mapEntityLeaf, kind),
            NPCControl.ActionBehaviors.FleeFromPlayer => new FleeFromPlayerMapEntityBehavior(_mapEntityLeaf, kind),
            NPCControl.ActionBehaviors.TurnRandomly => new SpriteFlipMapEntityBehavior(_mapEntityLeaf, kind),
            NPCControl.ActionBehaviors.Wander => new WanderMapEntityBehavior(_mapEntityLeaf, kind),
            NPCControl.ActionBehaviors.FaceAwayFromPlayer => new FaceDirectionMapEntityBehavior(_mapEntityLeaf, kind),
            NPCControl.ActionBehaviors.TurnFixedInterval => new SpriteFlipMapEntityBehavior(_mapEntityLeaf, kind),
            NPCControl.ActionBehaviors.Disguise => new DisguiseMapEntityBehavior(_mapEntityLeaf, kind),
            NPCControl.ActionBehaviors.DisguiseOnce => new DisguiseOnceBeforeWanderMapEntityBehavior(_mapEntityLeaf, kind),
            NPCControl.ActionBehaviors.FaceAhead => new FaceDirectionMapEntityBehavior(_mapEntityLeaf, kind),
            NPCControl.ActionBehaviors.FaceBehind => new FaceDirectionMapEntityBehavior(_mapEntityLeaf, kind),
            NPCControl.ActionBehaviors.FaceUp => new FaceDirectionMapEntityBehavior(_mapEntityLeaf, kind),
            NPCControl.ActionBehaviors.FaceDown => new FaceDirectionMapEntityBehavior(_mapEntityLeaf, kind),
            NPCControl.ActionBehaviors.SetPath => new MoveAlongPathMapEntityBehavior(_mapEntityLeaf, kind),
            NPCControl.ActionBehaviors.ChargeAtPlayer => new ChargeAtPlayerMapEntityBehavior(_mapEntityLeaf, kind),
            NPCControl.ActionBehaviors.ChargeAtPlayerFlipSprite => new ChargeAtPlayerMapEntityBehavior(
                _mapEntityLeaf,
                kind),
            NPCControl.ActionBehaviors.ShootProjectile => new ShootProjectileMapEntityBehavior(_mapEntityLeaf, kind),
            NPCControl.ActionBehaviors.ChargeAndAttack => new ChaseAndAttackPlayerMapEntityBehavior(_mapEntityLeaf, kind),
            NPCControl.ActionBehaviors.Unmoveable => new UnmovableWhenDizzyMapEntityBehavior(_mapEntityLeaf, kind),
            NPCControl.ActionBehaviors.ChargeAttackUnderground => new ChaseAndAttackPlayerMapEntityBehavior(
                _mapEntityLeaf,
                kind),
            NPCControl.ActionBehaviors.WanderUnderground => new WanderMapEntityBehavior(_mapEntityLeaf, kind),
            NPCControl.ActionBehaviors.StealthAI => (int)_mapEntityLeaf.InternalOutOfRangeActionFrequency == 5555
                ? CreateNewStealthSpotWhileAsleepBehaviorFromExisting(registryResolver)
                : CreateNewStealthSpotBehaviorFromExisting(registryResolver),
            NPCControl.ActionBehaviors.SetPathJump => new MoveAlongPathMapEntityBehavior(_mapEntityLeaf, kind),
            NPCControl.ActionBehaviors.ChangeSpriteInRandius => new ChangeAnimstateInRadiusMapEntityBehavior(
                _mapEntityLeaf),
            NPCControl.ActionBehaviors.ChaseWhenAnim => new ChasePlayerWhenAnimstateIsChaseMapEntityBehavior(
                _mapEntityLeaf,
                kind),
            NPCControl.ActionBehaviors.WalkWhenAnim => new WanderWhenAnimstateIsWalkOrIdleMapEntityBehavior(
                _mapEntityLeaf,
                kind),
            NPCControl.ActionBehaviors.WanderOffscreen => new WanderMapEntityBehavior(_mapEntityLeaf, kind),
            NPCControl.ActionBehaviors.WanderNoWarp => new WanderMapEntityBehavior(_mapEntityLeaf, kind),
            NPCControl.ActionBehaviors.WanderOnWater => new WanderMapEntityBehavior(_mapEntityLeaf, kind),
            NPCControl.ActionBehaviors.ChaseOnWater => new ChasePlayerMapEntityBehavior(_mapEntityLeaf, kind),
            _ => ThrowHelper.ThrowInvalidOperationException<MapEntityBehavior?>(
                $"The internal action behavior type {internalType} is not supported.")
        };
    }

    private StealthSpotWhileAsleepBehavior CreateNewStealthSpotWhileAsleepBehaviorFromExisting(
        IRegistryResolver registryResolver)
    {
        StealthSpotWhileAsleepBehavior behavior = new(_mapEntityLeaf);
        behavior.InitializeFromExisting(registryResolver);
        return behavior;
    }

    private StealthSpotBehavior CreateNewStealthSpotBehaviorFromExisting(IRegistryResolver registryResolver)
    {
        StealthSpotBehavior behavior = new(_mapEntityLeaf);
        behavior.InitializeFromExisting(registryResolver);
        return behavior;
    }

    public bool HasBehavior<TBehavior>() where TBehavior : MapEntityBehavior =>
        OutOfBehaviorRangeBehavior is TBehavior || InBehaviorRangeBehavior is TBehavior;

    public TBehavior? TryGetBehavior<TBehavior>(BehaviorKind kind) where TBehavior : MapEntityBehavior
    {
        return kind switch
        {
            BehaviorKind.OutOfRange => OutOfBehaviorRangeBehavior as TBehavior,
            BehaviorKind.InRange => InBehaviorRangeBehavior as TBehavior,
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<TBehavior?>(nameof(kind))
        };
    }

    public FaceDirectionMapEntityBehavior SetFaceDirectionBehavior(
        BehaviorKind kind,
        FacingBehaviorDirection direction)
    {
        FaceDirectionMapEntityBehavior behavior = new(_mapEntityLeaf, kind) { FacingDirection = direction };
        SetActionBehavior(behavior, kind);
        return behavior;
    }

    public ChasePlayerMapEntityBehavior SetChasePlayerBehavior(
        BehaviorKind kind,
        float movementSpeedMultiplier,
        bool chaseOnWater)
    {
        ChasePlayerMapEntityBehavior behavior = new(_mapEntityLeaf, kind)
        {
            ChaseOnWater = chaseOnWater,
            MovementSpeedMultiplier = movementSpeedMultiplier
        };
        SetActionBehavior(behavior, kind);
        return behavior;
    }

    public ChasePlayerWhenAnimstateIsChaseMapEntityBehavior SetChasePlayerWhenAnimstateIsChaseBehavior(
        BehaviorKind kind,
        int animstateOverrideWhenNotChase,
        float movementSpeedMultiplier)
    {
        ChasePlayerWhenAnimstateIsChaseMapEntityBehavior behavior = new(_mapEntityLeaf, kind)
        {
            AnimstateOverrideWhenNotChase = animstateOverrideWhenNotChase,
            MovementSpeedMultiplier = movementSpeedMultiplier
        };
        SetActionBehavior(behavior, kind);
        return behavior;
    }

    public ChaseAndAttackPlayerMapEntityBehavior SetChaseAndAttackPlayerBehavior(
        BehaviorKind kind,
        float minimumDistanceFromPlayerBeforeAttacking,
        float movementSpeedMultiplier,
        bool attackFromUnderground)
    {
        ChaseAndAttackPlayerMapEntityBehavior behavior = new(_mapEntityLeaf, kind)
        {
            MinimumDistanceFromPlayerBeforeAttacking = minimumDistanceFromPlayerBeforeAttacking,
            AttackFromUnderground = attackFromUnderground,
            MovementSpeedMultiplier = movementSpeedMultiplier
        };
        SetActionBehavior(behavior, kind);
        return behavior;
    }

    public FleeFromPlayerMapEntityBehavior SetFleeFromPlayerBehavior(BehaviorKind kind)
    {
        FleeFromPlayerMapEntityBehavior behavior = new(_mapEntityLeaf, kind);
        SetActionBehavior(behavior, kind);
        return behavior;
    }

    public SpriteFlipMapEntityBehavior SetSpriteFlipBehavior(
        BehaviorKind kind,
        float baseFlipIntervalInFrames,
        bool flipsAtRandomInterval)
    {
        SpriteFlipMapEntityBehavior behavior = new(_mapEntityLeaf, kind)
        {
            BaseFlipIntervalInFrames = baseFlipIntervalInFrames,
            FlipsAtRandomInterval = flipsAtRandomInterval
        };
        SetActionBehavior(behavior, kind);
        return behavior;
    }

    public WanderMapEntityBehavior SetWanderBehavior(
        BehaviorKind kind,
        WanderBehaviorPattern pattern,
        float radiusToWanderFromStartingPosition,
        float maxFramesIntervalBeforeMovingAgain,
        float maxDistanceFromStartingPositionBeforeTeleport)
    {
        WanderMapEntityBehavior behavior = new(_mapEntityLeaf, kind)
        {
            WanderPattern = pattern,
            MaxFramesIntervalBeforeMovingAgain = maxFramesIntervalBeforeMovingAgain,
            RadiusToWanderFromStartingPosition = radiusToWanderFromStartingPosition,
            MaxDistanceFromStartingPositionBeforeTeleport = maxDistanceFromStartingPositionBeforeTeleport
        };
        SetActionBehavior(behavior, kind);
        return behavior;
    }

    // NOTE: The animstate AND the wander frames delay are the same so they conflict, but there's not much that can be done to address this
    public WanderWhenAnimstateIsWalkOrIdleMapEntityBehavior SetWanderWhenAnimstateIsWalkOrIdleBehavior(
        BehaviorKind kind,
        int animstateOverrideWhenNotWalkOrIdle,
        float radiusToWanderFromStartingPosition,
        float maxDistanceFromStartingPositionBeforeTeleport)
    {
        WanderWhenAnimstateIsWalkOrIdleMapEntityBehavior behavior = new(_mapEntityLeaf, kind)
        {
            AnimstateOverrideWhenNotWalkOrIdle = animstateOverrideWhenNotWalkOrIdle,
            RadiusToWanderFromStartingPosition = radiusToWanderFromStartingPosition,
            MaxDistanceFromStartingPositionBeforeTeleport = maxDistanceFromStartingPositionBeforeTeleport
        };
        SetActionBehavior(behavior, kind);
        return behavior;
    }

    public DisguiseMapEntityBehavior SetDisguiseBehavior(BehaviorKind kind)
    {
        DisguiseMapEntityBehavior behavior = new(_mapEntityLeaf, kind);
        SetActionBehavior(behavior, kind);
        return behavior;
    }

    public DisguiseOnceBeforeWanderMapEntityBehavior SetDisguiseOnceBeforeWanderBehavior(
        BehaviorKind kind,
        float radiusToWanderFromStartingPosition,
        float maxFramesIntervalBeforeMovingAgain,
        float maxDistanceFromStartingPositionBeforeTeleport)
    {
        DisguiseOnceBeforeWanderMapEntityBehavior behavior = new(_mapEntityLeaf, kind)
        {
            MaxFramesIntervalBeforeMovingAgain = maxFramesIntervalBeforeMovingAgain,
            RadiusToWanderFromStartingPosition = radiusToWanderFromStartingPosition,
            MaxDistanceFromStartingPositionBeforeTeleport = maxDistanceFromStartingPositionBeforeTeleport
        };
        SetActionBehavior(behavior, kind);
        return behavior;
    }

    public MoveAlongPathMapEntityBehavior SetMoveAlongPathBehavior(
        BehaviorKind kind,
        float delayFramesBeforeMovingToNextNode,
        bool jumpWhileMoving,
        ICollection<Vector3> positionNodesInPath)
    {
        MoveAlongPathMapEntityBehavior behavior = new(_mapEntityLeaf, kind)
        {
            DelayFramesBeforeMovingToNextNode = delayFramesBeforeMovingToNextNode,
            JumpWhileMoving = jumpWhileMoving,
        };
        foreach (Vector3 vector3 in positionNodesInPath)
            behavior.MovementPathNodePositions.Add(vector3);
        SetActionBehavior(behavior, kind);
        return behavior;
    }

    public ChargeAtPlayerMapEntityBehavior SetChargeAtPlayerBehavior(
        BehaviorKind kind,
        float movementSpeedMultiplier,
        bool lockSpriteFlipDuringCharge)
    {
        ChargeAtPlayerMapEntityBehavior behavior = new(_mapEntityLeaf, kind)
        {
            LockSpriteFlipDuringCharge = lockSpriteFlipDuringCharge,
            MovementSpeedMultiplier = movementSpeedMultiplier
        };
        SetActionBehavior(behavior, kind);
        return behavior;
    }

    public ShootProjectileMapEntityBehavior SetShootProjectileBehavior(BehaviorKind kind)
    {
        ShootProjectileMapEntityBehavior behavior = new(_mapEntityLeaf, kind);
        SetActionBehavior(behavior, kind);
        return behavior;
    }

    public UnmovableWhenDizzyMapEntityBehavior SetUnmovableWhenDizzyBehavior(BehaviorKind kind)
    {
        UnmovableWhenDizzyMapEntityBehavior behavior = new(_mapEntityLeaf, kind);
        SetActionBehavior(behavior, kind);
        return behavior;
    }

    public ChangeAnimstateInRadiusMapEntityBehavior SetChangeAnimstateInRadiusGlobalBehavior(
        float radius,
        int animstateWhenOutsideRadius,
        int animstateWhenInsideRadius)
    {
        ChangeAnimstateInRadiusMapEntityBehavior behavior = new(_mapEntityLeaf)
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
        int visionLengthInUnits,
        float delayFramesBeforeMovingToNextNode)
    {
        StealthSpotBehavior behavior = new(_mapEntityLeaf)
        {
            EventToStartWhenSpottingPlayer = eventToStartWhenSpottingPlayer,
            VisionLengthInUnits = visionLengthInUnits,
            DelayFramesBeforeMovingToNextNode = delayFramesBeforeMovingToNextNode
        };
        SetActionBehavior(null, BehaviorKind.InRange);
        SetActionBehavior(behavior, BehaviorKind.OutOfRange);
        return behavior;
    }

    public StealthSpotWhileAsleepBehavior SetStealthSpotWhileAsleepGlobalBehavior(
        Branch<EventLeaf>? eventToStartWhenSpottingPlayer,
        int visionLengthInUnits)
    {
        StealthSpotWhileAsleepBehavior behavior = new(_mapEntityLeaf)
        {
            EventToStartWhenSpottingPlayer = eventToStartWhenSpottingPlayer,
            VisionLengthInUnits = visionLengthInUnits,
        };
        SetActionBehavior(null, BehaviorKind.InRange);
        SetActionBehavior(behavior, BehaviorKind.OutOfRange);
        return behavior;
    }

    public void ClearBehavior(BehaviorKind kind)
    {
        SetActionBehavior(null, kind);
    }

    public void ClearAllBehaviors()
    {
        SetActionBehavior(null, null);
        _mapEntityLeaf.InternalOutOfRangeActionFrequency = 0f;
        _mapEntityLeaf.InternalInRangeActionFrequency = 0f;
    }

    private void SetActionBehavior(MapEntityBehavior? behavior, BehaviorKind? kind)
    {
        switch (kind)
        {
            case BehaviorKind.OutOfRange:
                OutOfBehaviorRangeBehavior = behavior;
                if (behavior is null)
                {
                    _mapEntityLeaf.InternalOutOfRangeBehavior = NPCControl.ActionBehaviors.None;
                    _mapEntityLeaf.InternalOutOfRangeActionFrequency = 0f;
                }

                if (InBehaviorRangeBehavior is ChasePlayerWhenAnimstateIsChaseMapEntityBehavior)
                {
                    InBehaviorRangeBehavior = null;
                    _mapEntityLeaf.InternalInRangeBehavior = NPCControl.ActionBehaviors.None;
                    _mapEntityLeaf.InternalInRangeActionFrequency = 0f;
                }

                break;
            case BehaviorKind.InRange:
                InBehaviorRangeBehavior = behavior;
                if (behavior is null)
                {
                    _mapEntityLeaf.InternalInRangeBehavior = NPCControl.ActionBehaviors.None;
                    _mapEntityLeaf.InternalInRangeActionFrequency = 0f;
                }

                if (OutOfBehaviorRangeBehavior is ChasePlayerWhenAnimstateIsChaseMapEntityBehavior)
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