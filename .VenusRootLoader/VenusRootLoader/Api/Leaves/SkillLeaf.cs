namespace VenusRootLoader.Api.Leaves;

// TODO: Need to patch RefreshSkills which requires additional conditions support and linking with RankBonus leaves
internal sealed class SkillLeaf : Leaf
{
    internal enum BoolCasing
    {
        AllPascalCase,
        AllCamelCase,
        PascalCaseWithLastTwoCamelCase
    }
    
    internal enum SkillCostResource
    {
        Tp,
        Hp
    }

    internal enum SkillTarget
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

    internal sealed class SkillLanguageData
    {
        internal string Name { get; set; } = "";
        internal string Description { get; set; } = "";
    }

    internal SkillLeaf(int gameId, string namedId, string creatorId) : base(gameId, namedId, creatorId)
    {
    }

    internal BoolCasing OriginalBoolCasing { get; set; }

    internal LocalizedData<SkillLanguageData> LocalizedData { get; } = new();
    internal SkillCostResource CostResource { get; set; }
    internal int Cost { get; set; }
    internal SkillTarget Target { get; set; }
    internal SkillUsability UsableBy { get; set; }
    internal Branch<ActionCommandHelpTextLeaf>? ActionCommandHelpText { get; set; }
}