using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Unity;

namespace VenusRootLoader.Api.Leaves;

public sealed class EnemyLeaf : Leaf, IEnemyPortraitSprite
{
    private const int DeathTypeKoWithReserveDuplicateId = 4;

    public enum EnemyDeathType
    {
        SpinSmoke = 0,
        SpinNoSmoke = 1,
        KoWithReserve = 2,
        SpinSmokeWithReserve = 3,
        SpinKoWithReserve = 5,
        Shrink = 6,
        ShrinkNoSmoke = 7,
        None = 8,
        Sink = 9,
        ExplodeAnim = 10,
        DropSprites = 11,
        NoneWithDestroyOnNextMainTurn = 12
    }

    public enum AutoHitActionTrigger
    {
        WhenDamaged = 1,
        WhenDamagedWhileFlying = 2,
        WhenDamagedWhileOnGround = 3
    }

    public sealed class EnemyLanguageData
    {
        public string Name { get; set; } = "";
        public List<string> PaginatedBiography { get; init; } = new();
        public string BeeSpyDialogue { get; set; } = "beetattle";
        public string BeetleSpyDialogue { get; set; } = "beetletattle";
        public string MothSpyDialogue { get; set; } = "mothtattle";
    }

    public Dictionary<int, EnemyLanguageData> LanguageData { get; } = new();

    int? IEnemyPortraitSprite.EnemyPortraitsSpriteIndex { get; set; }
    WrappedSprite IEnemyPortraitSprite.WrappedSprite { get; set; } = new();

    public Sprite? PortraitSprite
    {
        get => ((IEnemyPortraitSprite)this).WrappedSprite.Sprite;
        set => ((IEnemyPortraitSprite)this).WrappedSprite.Sprite = value;
    }

    public bool CanBeSpied { get; internal set; } = true;

    public int EntityAnimId { get; set; }
    public int BaseMaxHp { get; set; }
    public int BaseDefense { get; set; }
    public int BaseExpReward { get; set; }
    public int BaseBerriesDropAmount { get; set; }
    public Vector3 CursorOffset { get; set; } = Vector3.zero;
    public int PoisonResistance { get; set; }
    public int FreezeResistance { get; set; }
    public int NumbResistance { get; set; }
    public int SleepResistance { get; set; }
    public float LogicalSize { get; set; }
    public Vector3 EntityFreezeSize { get; set; } = Vector3.one;
    public Vector3 EntityFreezeOffset { get; set; } = Vector3.zero;
    public BattleControl.BattlePosition StartingBattlePosition { get; set; }
    public float EntityHeight { get; set; }
    public float EntityBobSpeed { get; set; }
    public float EntityBobRange { get; set; }
    public List<BattleControl.AttackProperty> Properties { get; } = new();
    public float Weight { get; set; }
    public Branch<EnemyLeaf>? BaseEnemyId { get; internal set; }
    public int? EventIdOnDeath { get; set; }
    public int ActorTurnAmountPerMainTurn { get; set; }
    public bool CanBeTaunted { get; set; } = true;
    public bool CanFall { get; set; } = true;
    public bool HasFixedExpScaling { get; set; }
    public bool IsAffectedByExhaustion { get; set; } = true;
    public bool HasStatsHiddenFromHud { get; set; }

    internal int InternalDeathType { get; set; }

    public EnemyDeathType DeathType
    {
        get => InternalDeathType == DeathTypeKoWithReserveDuplicateId
            ? EnemyDeathType.KoWithReserve
            : (EnemyDeathType)InternalDeathType;
        set => InternalDeathType = (int)value;
    }

    public List<Branch<EnemyLeaf>> EnemiesWhoTriggerHitActionWhenDamaged { get; } = new();
    public int HardModeAttackIncrease { get; set; }
    public int HardModeBaseMaxHpIncrease { get; set; }
    public int HardModeBaseDefenseIncrease { get; set; }
    public int DefenseIncreaseWhenDefending { get; set; }
    public Vector3 ItemOffset { get; set; } = Vector3.zero;
    public bool IsBaseStateBattleIdle { get; set; }
    public int? EventIdOnFall { get; set; }
    public AutoHitActionTrigger HitActionTrigger { get; set; }
    public bool CanActWhileStunned { get; set; }
    public float SizeWhenFrozen { get; set; }

    public bool IsIncludedInRandomCaveOfTrialsPool { get; set; } = true;
    public bool IsRareSpyData { get; set; }
}