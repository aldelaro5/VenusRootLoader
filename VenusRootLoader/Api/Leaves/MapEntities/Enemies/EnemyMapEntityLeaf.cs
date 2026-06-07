using UnityEngine;
using VenusRootLoader.Api.Leaves.MapEntities.Behaviors;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Api.Leaves.MapEntities.Enemies;

public abstract class EnemyMapEntityLeaf : MapEntityLeaf
{
    internal sealed override NPCControl.NPCType Type => NPCControl.NPCType.Enemy;
    internal sealed override NPCControl.ObjectTypes ObjectType => NPCControl.ObjectTypes.None;
    internal sealed override NPCControl.Interaction Interaction => NPCControl.Interaction.None;

    protected EnemyMapEntityLeaf(int gameId, string namedId, string creatorId) : base(gameId, namedId, creatorId)
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

    public float EntityBobSpeed { get => InternalBobSpeed; set => InternalBobSpeed = value; }
    public float EntityBobRange { get => InternalBobRange; set => InternalBobRange = value; }
    public float EntityInitialHeight { get => InternalInitialHeight; set => InternalInitialHeight = value; }
    public float EntityCapsuleColliderRadius { get => InternalCcolRadius; set => InternalCcolRadius = value; }
    public float EntityCapsulerColliderHeight { get => InternalCcolHeight; set => InternalCcolHeight = value; }
    public bool EntitySpriteIsFlipped { get => InternalIsFlipped; set => InternalIsFlipped = value; }
    public Vector3 EntityEmoticonOffset { get => InternalEmoticonOffset; set => InternalEmoticonOffset = value; }
    public Vector3 EntityIceCubeOffset { get => InternalFreezeOffset; set => InternalFreezeOffset = value; }
    public Vector3 EntityIceCubeSize { get => InternalFreezeSize; set => InternalFreezeSize = value; }
    public bool ReturnToHeightWhenUnfrozen { get => InternalReturnToHeight; set => InternalReturnToHeight = value; }
    public float MovementRadius { get => InternalRadiusLimit; set => InternalRadiusLimit = value; }
    public float BehaviorAndInteractRangeRadius { get => InternalRadius; set => InternalRadius = value; }
    public float ExtraFreezeTimeInFrames { get => InternalFreezeTime; set => InternalFreezeTime = value; }
    public float EntitySpeed { get => InternalSpeed; set => InternalSpeed = value; }
    public float BehaviorRangeRadius { get => InternalRadius; set => InternalRadius = value; }

    private readonly ListRefWrapper<Branch<EnemyLeaf>, int> _enemiesFormationInBattle;
    public IList<Branch<EnemyLeaf>> EnemiesFormationInBattle => _enemiesFormationInBattle;

    internal override void InitializeFromNew()
    {
        InternalAnimIdOrItemId = 0;
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