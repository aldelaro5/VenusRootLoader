using CommunityToolkit.Diagnostics;
using VenusRootLoader.Registry;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves.RankBonuses;

[ExposeCreateFromVenus(typeof(RankBonusLeaf))]
public sealed class SkillLearnRankBonusLeaf : RankBonusLeaf
{
    internal SkillLearnRankBonusLeaf(int gameId, string creatorId, string namedId)
        : base(gameId, creatorId, namedId)
    {
    }

    internal override RankBonusType InternalBonusType => RankBonusType.GrantSkill;

    public SkillLearner Learner { get; set; } = SkillLearner.AllSkillUsers;

    private Branch<SkillLeaf> _internalSkill = null!;

    public Branch<SkillLeaf> Skill
    {
        get => _internalSkill;
        set
        {
            _internalSkill = value;
            InternalSecondParameter = value.Resolve().GameId;
        }
    }

    internal override void InitializeFromExisting()
    {
        _internalSkill = RegistryResolver.Resolve<SkillLeaf>().GetByGameId(InternalSecondParameter);
    }

    [LeafInitializeFromNew]
    internal void InitializeFromNew(int rankNeeded, Branch<SkillLeaf> skill)
    {
        base.InitializeFromNew(rankNeeded);
        Skill = skill;
    }

    internal HashSet<int> GetSkillLearnerTrueIds()
    {
        if (Learner == SkillLearner.AllSkillUsers)
        {
            return Skill.Resolve().UsableBy switch
            {
                SkillLeaf.SkillUsability.AnyBug => [0, 1, 2],
                SkillLeaf.SkillUsability.AnyBugWithAtLeastOneValidEnemyTarget => [0, 1, 2],
                SkillLeaf.SkillUsability.Bee => [0],
                SkillLeaf.SkillUsability.Beetle => [1],
                SkillLeaf.SkillUsability.Moth => [2],
                SkillLeaf.SkillUsability.BeeAndBeetle => [0, 1],
                SkillLeaf.SkillUsability.BeeAndMoth => [0, 2],
                SkillLeaf.SkillUsability.BeetleAndMoth => [1, 2],
                _ => ThrowHelper.ThrowArgumentOutOfRangeException<HashSet<int>>()
            };
        }

        return Learner switch
        {
            SkillLearner.Bee => [0],
            SkillLearner.Beetle => [1],
            SkillLearner.Moth => [2],
            SkillLearner.BeeAndBeetle => [0, 1],
            SkillLearner.BeeAndMoth => [0, 2],
            SkillLearner.BeetleAndMoth => [1, 2],
            SkillLearner.All => [0, 1, 2],
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<HashSet<int>>()
        };
    }
}