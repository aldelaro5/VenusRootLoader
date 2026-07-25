using UnityEngine;
using VenusRootLoader.Api.Leaves.MapEntities.Behaviors;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Enemies;

public abstract class EnemyEncounterMapEntityLeaf : MapEntityLeaf
{
    internal sealed override NPCControl.NPCType Type => NPCControl.NPCType.Enemy;
    internal sealed override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.None;
    internal sealed override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

    protected EnemyEncounterMapEntityLeaf(int gameId, string namedId, string creatorId) : base(
        gameId,
        namedId,
        creatorId)
    {
        _enemiesFormationInBattle = new(InternalBattleEnemyIds, 0, x => new(x.GameId));
        BehaviorSystem = new(this);
    }

    public MapEntityBehaviorSystem BehaviorSystem { get; }

    public Branch<AnimIdLeaf> AnimId
    {
        get;
        set
        {
            InternalAnimIdOrItemId = value.GameId;
            field = value;
        }
    }

    public float EntityInitialAltitudeFromGround { get => InternalInitialHeight; set => InternalInitialHeight = value; }

    // This is not a mistake, the game mislabels them by swapping their semantics
    public float EntityBobRangeInUnitsWhileAirborne { get => InternalBobSpeed; set => InternalBobSpeed = value; }
    public float EntityBobSpeedWhileAirborne { get => InternalBobRange; set => InternalBobRange = value; }

    public float EntityCapsuleColliderRadius { get => InternalCcolRadius; set => InternalCcolRadius = value; }
    public float EntityCapsulerColliderHeight { get => InternalCcolHeight; set => InternalCcolHeight = value; }
    public bool EntitySpriteStartsFlipped { get => InternalIsFlipped; set => InternalIsFlipped = value; }
    public Vector3 EntityEmoticonOffset { get => InternalEmoticonOffset; set => InternalEmoticonOffset = value; }
    public Vector3 EntityIceCubeOffsetWhenFrozen { get => InternalFreezeOffset; set => InternalFreezeOffset = value; }
    public Vector3 EntityIceCubeSizeWhenFrozen { get => InternalFreezeSize; set => InternalFreezeSize = value; }
    public bool ReturnToAirWhenUnfrozen { get => InternalReturnToHeight; set => InternalReturnToHeight = value; }
    public float MovementRadius { get => InternalRadiusLimit; set => InternalRadiusLimit = value; }
    public float ExtraFreezeTimeInFrames { get => InternalFreezeTime; set => InternalFreezeTime = value; }
    public float EntityMovementSpeed { get => InternalSpeed; set => InternalSpeed = value; }
    public float BehaviorSystemRangeRadius { get => InternalRadius; set => InternalRadius = value; }

    private readonly ListRefWrapper<Branch<EnemyLeaf>, int> _enemiesFormationInBattle;
    public IList<Branch<EnemyLeaf>> EnemiesFormationInBattle => _enemiesFormationInBattle;

    public bool HasDoubledForceMoveFailsafeTimer
    {
        get => Modifiers.HasFlag(MapEntityModifiers.TIME);
        set
        {
            if (value)
                Modifiers |= MapEntityModifiers.TIME;
            else
                Modifiers &= ~MapEntityModifiers.TIME;
        }
    }

    public bool ForceExtraFreezeEffect
    {
        get => Modifiers.HasFlag(MapEntityModifiers.ICE);
        set
        {
            if (value)
                Modifiers |= MapEntityModifiers.ICE;
            else
                Modifiers &= ~MapEntityModifiers.ICE;
        }
    }

    public bool HasSpriteFlippingUpdatesEvenWhenOutOfCameraRange
    {
        get => Modifiers.HasFlag(MapEntityModifiers.ALF);
        set
        {
            if (value)
                Modifiers |= MapEntityModifiers.ALF;
            else
                Modifiers &= ~MapEntityModifiers.ALF;
        }
    }

    internal virtual void InitializeFromNew(
        Vector3 startingPosition,
        Branch<AnimIdLeaf> animId,
        IList<Branch<EnemyLeaf>> enemiesFormationInBattle)
    {
        AnimId = animId;
        EntityStartingPosition = startingPosition;
        foreach (Branch<EnemyLeaf> enemies in enemiesFormationInBattle)
            EnemiesFormationInBattle.Add(enemies);
    }

    internal override void InitializeFromExisting(IRegistryResolver registryResolver)
    {
        ILeavesRegistry<EnemyLeaf> enemiesRegistry = registryResolver.Resolve<EnemyLeaf>();
        ILeavesRegistry<AnimIdLeaf> animIdsRegistry = registryResolver.Resolve<AnimIdLeaf>();

        BehaviorSystem.InitializeBehaviorFromExisting(registryResolver);

        AnimId = new(animIdsRegistry.LeavesByGameIds[InternalAnimIdOrItemId]);

        _enemiesFormationInBattle.SynchronizeFromExistingData(
            InternalBattleEnemyIds
                .Select(i => new Branch<EnemyLeaf>(enemiesRegistry.LeavesByGameIds[i.Value]))
                .ToList());
    }
}