using System.Collections;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves;

// TODO: Need linking with RankBonus leaves
[ExposeFromVenus(typeof(MainManager.Skills))]
public sealed class SkillLeaf : Leaf
{
    internal enum BoolCasing
    {
        AllPascalCase,
        AllCamelCase,
        PascalCaseWithLastTwoCamelCase
    }

    public enum SkillCostResource
    {
        Tp,
        Hp
    }

    public enum SkillTarget
    {
        SingleEnemy,
        SingleEnemyGround,
        SingleEnemyFront,
        SingleEnemyGroundFront,
        AllEnemies,
        AllEnemiesGround,
        SingleAlly,
        SingleAllyAlive,
        SingleAllyAliveExcludingSelf,
        SingleAllyFaintedExcludingSelf,
        AllParty,
        All,
        None,
        User
    }

    public enum SkillUsability
    {
        AnyBug,
        AnyBugWithAtLeastOneValidEnemyTarget,
        Bee,
        Beetle,
        Moth,
        BeeAndBeetle,
        BeeAndMoth,
        BeetleAndMoth,
    }

    public sealed class SkillLanguageData
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
    }

    public delegate bool SkillUsabilityPredicate(MainManager.RefreshSkillsContext context);

    public delegate IEnumerator PlayerAction(BattleControl battle, BattleControl.DoActionPlayerActionContext context);

    internal SkillLeaf(int gameId, string creatorId, string namedId) : base(gameId, creatorId, namedId)
    {
    }

    internal BoolCasing OriginalBoolCasing { get; set; }

    public LocalizedData<SkillLanguageData> LocalizedData { get; } = new();
    public SkillCostResource CostResource { get; set; }
    public int Cost { get; set; }
    public SkillTarget Target { get; set; }
    public SkillUsability UsableBy { get; set; }
    public Branch<SkillActionCommandHelpTextLeaf>? ActionCommandHelpText { get; set; }

    public SkillUsabilityPredicate? UsabilityPredicate { get; set; }
    public PlayerAction? Action { get; set; }

    [LeafInitializeFromNew]
    internal void InitializeFromNew(SkillUsabilityPredicate predicate, PlayerAction action)
    {
        UsabilityPredicate = predicate;
        Action = action;
    }
}