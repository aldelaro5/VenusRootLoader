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

        MapEntityBehavior? behavior = MapExistingInternalBehaviorType(kind, internalType, registryResolver);

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

    private MapEntityBehavior? MapExistingInternalBehaviorType(
        ActionBehaviorKind kind,
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

    public TBehavior? TryGetBehavior<TBehavior>(ActionBehaviorKind kind) where TBehavior : MapEntityBehavior
    {
        return kind switch
        {
            ActionBehaviorKind.OutOfRange => OutOfBehaviorRangeBehavior as TBehavior,
            ActionBehaviorKind.InRange => InBehaviorRangeBehavior as TBehavior,
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<TBehavior?>(nameof(kind))
        };
    }

    public FaceDirectionMapEntityBehavior SetFaceDirectionBehavior(
        ActionBehaviorKind kind,
        FacingBehaviorDirection direction)
    {
        FaceDirectionMapEntityBehavior behavior = new(_mapEntityLeaf, kind) { FacingDirection = direction };
        SetActionBehavior(behavior, kind);
        return behavior;
    }

    public ChasePlayerMapEntityBehavior SetChasePlayerBehavior(
        ActionBehaviorKind kind,
        bool chaseOnWater,
        float movementSpeedMultiplier)
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
        ActionBehaviorKind kind,
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
        ActionBehaviorKind kind,
        float minimumDistanceFromPlayerBeforeAttacking,
        bool attacksFromUnderground,
        float movementSpeedMultiplier)
    {
        ChaseAndAttackPlayerMapEntityBehavior behavior = new(_mapEntityLeaf, kind)
        {
            MinimumDistanceFromPlayerBeforeAttacking = minimumDistanceFromPlayerBeforeAttacking,
            AttacksFromUnderground = attacksFromUnderground,
            MovementSpeedMultiplier = movementSpeedMultiplier
        };
        SetActionBehavior(behavior, kind);
        return behavior;
    }

    public FleeFromPlayerMapEntityBehavior SetFleeFromPlayerBehavior(ActionBehaviorKind kind)
    {
        FleeFromPlayerMapEntityBehavior behavior = new(_mapEntityLeaf, kind);
        SetActionBehavior(behavior, kind);
        return behavior;
    }

    public SpriteFlipMapEntityBehavior SetSpriteFlipBehavior(
        ActionBehaviorKind kind,
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
        ActionBehaviorKind kind,
        WanderBehaviorPattern pattern,
        float maxFramesIntervalBeforeMovingAgain,
        float radiusToWanderFromStartingPosition,
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
        ActionBehaviorKind kind,
        int animstateOverrideWhenNotChase,
        float radiusToWanderFromStartingPosition,
        float maxDistanceFromStartingPositionBeforeTeleport)
    {
        WanderWhenAnimstateIsWalkOrIdleMapEntityBehavior behavior = new(_mapEntityLeaf, kind)
        {
            AnimstateOverrideWhenNotChase = animstateOverrideWhenNotChase,
            RadiusToWanderFromStartingPosition = radiusToWanderFromStartingPosition,
            MaxDistanceFromStartingPositionBeforeTeleport = maxDistanceFromStartingPositionBeforeTeleport
        };
        SetActionBehavior(behavior, kind);
        return behavior;
    }

    public DisguiseMapEntityBehavior SetDisguiseBehavior(ActionBehaviorKind kind)
    {
        DisguiseMapEntityBehavior behavior = new(_mapEntityLeaf, kind);
        SetActionBehavior(behavior, kind);
        return behavior;
    }

    public DisguiseOnceBeforeWanderMapEntityBehavior SetDisguiseOnceBeforeWanderBehavior(
        ActionBehaviorKind kind,
        float maxFramesIntervalBeforeMovingAgain,
        float radiusToWanderFromStartingPosition,
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
        ActionBehaviorKind kind,
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
        ActionBehaviorKind kind,
        bool lockSpriteFlipDuringCharge,
        float movementSpeedMultiplier)
    {
        ChargeAtPlayerMapEntityBehavior behavior = new(_mapEntityLeaf, kind)
        {
            LockSpriteFlipDuringCharge = lockSpriteFlipDuringCharge,
            MovementSpeedMultiplier = movementSpeedMultiplier
        };
        SetActionBehavior(behavior, kind);
        return behavior;
    }

    public ShootProjectileMapEntityBehavior SetShootProjectileBehavior(ActionBehaviorKind kind)
    {
        ShootProjectileMapEntityBehavior behavior = new(_mapEntityLeaf, kind);
        SetActionBehavior(behavior, kind);
        return behavior;
    }

    public UnmovableWhenDizzyMapEntityBehavior SetUnmovableWhenDizzyBehavior(ActionBehaviorKind kind)
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
        float delayFramesBeforeMovingToNextNode,
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

    public StealthSpotWhileAsleepBehavior SetStealthSpotWhileAsleepGlobalBehavior(
        Branch<EventLeaf>? eventToStartWhenSpottingPlayer,
        int visionLengthInUnits)
    {
        StealthSpotWhileAsleepBehavior behavior = new(_mapEntityLeaf)
        {
            EventToStartWhenSpottingPlayer = eventToStartWhenSpottingPlayer,
            VisionLengthInUnits = visionLengthInUnits,
        };
        SetActionBehavior(null, ActionBehaviorKind.InRange);
        SetActionBehavior(behavior, ActionBehaviorKind.OutOfRange);
        return behavior;
    }

    public void ClearBehavior(ActionBehaviorKind kind)
    {
        SetActionBehavior(null, kind);
    }

    public void ClearAllBehaviors()
    {
        SetActionBehavior(null, null);
        _mapEntityLeaf.InternalOutOfRangeActionFrequency = 0f;
        _mapEntityLeaf.InternalInRangeActionFrequency = 0f;
    }

    private void SetActionBehavior(MapEntityBehavior? behavior, ActionBehaviorKind? kind)
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

                if (InBehaviorRangeBehavior is ChasePlayerWhenAnimstateIsChaseMapEntityBehavior)
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