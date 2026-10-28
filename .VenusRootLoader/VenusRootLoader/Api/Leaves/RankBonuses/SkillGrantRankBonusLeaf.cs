using VenusRootLoader.Registry;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves.RankBonuses;

[ExposeCreateFromVenus(typeof(RankBonusLeaf))]
public sealed class SkillGrantRankBonusLeaf : RankBonusLeaf
{
    internal SkillGrantRankBonusLeaf(int gameId, string creatorId, string namedId)
        : base(gameId, creatorId, namedId)
    {
    }

    internal override RankBonusType InternalBonusType => RankBonusType.GrantSkill;

    public PartyMember Recipient
    {
        get => (PartyMember)InternalFirstParameter;
        set => InternalFirstParameter = (int)value;
    }

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
    internal void InitializeFromNew(int rankNeeded, PartyMember recipient, Branch<SkillLeaf> skill)
    {
        base.InitializeFromNew(rankNeeded);
        Recipient = recipient;
        Skill = skill;
    }
}