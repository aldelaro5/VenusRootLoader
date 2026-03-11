using CommunityToolkit.Diagnostics;
using System.Text;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Extensions;
using VenusRootLoader.Utility;
using static BattleControl;
using static VenusRootLoader.Api.Leaves.SkillLeaf;
using RawUsabilityParameters = (bool usableByBee, bool usableByBeetle, bool usableByMoth);
using RawTargetingParameters = (BattleControl.AttackArea attackAtrea, bool onlyGroundedEnemies, bool onlyFrontEnemy,
    bool onlyPlayersAlive, bool excludeSelf, bool onlyPlayersFainted);

namespace VenusRootLoader.Patching.Resources.TextAsset.Parsers.GlobalData;

internal class SkillTextAssetParser : ITextAssetParser<SkillLeaf>
{
    public string GetTextAssetSerializedString(string subPath, SkillLeaf leaf)
    {
        RawTargetingParameters targetingParameters = leaf.Target switch
        {
            SkillTarget.SingleEnemy => (AttackArea.SingleEnemy, false, false, false, false, false),
            SkillTarget.SingleEnemyGround => (AttackArea.SingleEnemy, true, false, false, false, false),
            SkillTarget.SingleEnemyFront => (AttackArea.SingleEnemy, false, true, false, false, false),
            SkillTarget.SingleEnemyGroundFront => (AttackArea.SingleEnemy, true, true, false, false, false),
            SkillTarget.AllEnemies => (AttackArea.AllEnemies, false, false, false, false, false),
            SkillTarget.AllEnemiesGround => (AttackArea.AllEnemies, true, false, false, false, false),
            SkillTarget.SingleAlly => (AttackArea.SingleAlly, false, false, false, false, false),
            SkillTarget.SingleAllyAlive => (AttackArea.SingleAlly, false, false, true, false, false),
            SkillTarget.SingleAllyAliveExcludingSelf => (AttackArea.SingleAlly, false, false, true, true, false),
            SkillTarget.SingleAllyFaintedExcludingSelf => (AttackArea.SingleAlly, false, false, false, true, true),
            SkillTarget.AllParty => (AttackArea.AllParty, false, false, false, false, false),
            SkillTarget.All => (AttackArea.All, false, false, false, false, false),
            SkillTarget.None => (AttackArea.None, false, false, false, false, false),
            SkillTarget.User => (AttackArea.User, false, false, false, false, false),
            _ => ThrowHelper.ThrowNotSupportedException<(AttackArea, bool, bool, bool, bool, bool)>(
                $"Invalid {nameof(SkillTarget)}: {leaf.Target}")
        };

        RawUsabilityParameters usability = leaf.UsableBy switch
        {
            SkillUsability.AnyBug => (false, false, false),
            SkillUsability.Moth => (false, false, true),
            SkillUsability.Beetle => (false, true, false),
            SkillUsability.BeetleAndMoth => (false, true, true),
            SkillUsability.Bee => (true, false, false),
            SkillUsability.BeeAndMoth => (true, false, true),
            SkillUsability.BeeAndBeetle => (true, true, false),
            SkillUsability.AnyBugWithAtLeastOneValidEnemyTarget => (true, true, true),
            _ => ThrowHelper.ThrowNotSupportedException<(bool, bool, bool)>(
                $"Invalid {nameof(SkillUsability)}: {leaf.UsableBy}")
        };

        StringBuilder sb = new();

        sb.Append(leaf.Cost * (leaf.CostResource == SkillCostResource.Tp ? 1 : -1));
        sb.Append('@');
        sb.Append(targetingParameters.attackAtrea.ToString());
        sb.Append('@');
        sb.Append(usability.usableByBee);
        sb.Append('@');
        sb.Append(usability.usableByBeetle);
        sb.Append('@');
        sb.Append(usability.usableByMoth);
        sb.Append('@');
        sb.Append(targetingParameters.onlyGroundedEnemies);
        sb.Append('@');
        sb.Append(targetingParameters.onlyFrontEnemy);
        sb.Append('@');
        sb.Append(leaf.HelpTextActionCommandGameId);
        sb.Append('@');
        sb.Append(targetingParameters.onlyPlayersAlive);
        sb.Append('@');
        sb.Append(targetingParameters.excludeSelf);
        sb.Append('@');
        sb.Append(targetingParameters.onlyPlayersFainted);

        return sb.ToString();
    }

    public void FromTextAssetSerializedString(string subPath, string text, SkillLeaf leaf)
    {
        string[] fields = text.Split(StringUtils.AtSymbolSplitDelimiter);
        RawUsabilityParameters usabilityParameters =
            (bool.Parse(fields[2]), bool.Parse(fields[3]), bool.Parse(fields[4]));
        RawTargetingParameters targetParameters = (Enum.Parse<AttackArea>(fields[1]), bool.Parse(fields[5]),
            bool.Parse(fields[6]), bool.Parse(fields[8]),
            bool.Parse(fields[9]), bool.Parse(fields[10]));

        int cost = int.Parse(fields[0]);
        leaf.CostResource = cost >= 0 ? SkillCostResource.Tp : SkillCostResource.Hp;
        leaf.Cost = Math.Abs(cost);
        leaf.HelpTextActionCommandGameId = int.Parse(fields[7]);

        leaf.Target = targetParameters switch
        {
            (AttackArea.SingleEnemy, false, false, false, false, false) => SkillTarget.SingleEnemy,
            (AttackArea.SingleEnemy, true, false, false, false, false) => SkillTarget.SingleEnemyGround,
            (AttackArea.SingleEnemy, false, true, false, false, false) => SkillTarget.SingleEnemyFront,
            (AttackArea.SingleEnemy, true, true, false, false, false) => SkillTarget.SingleEnemyGroundFront,
            (AttackArea.AllEnemies, false, false, false, false, false) => SkillTarget.AllEnemies,
            (AttackArea.AllEnemies, true, false, false, false, false) => SkillTarget.AllEnemiesGround,
            (AttackArea.SingleAlly, false, false, false, false, false) => SkillTarget.SingleAlly,
            (AttackArea.SingleAlly, false, false, true, false, false) => SkillTarget.SingleAllyAlive,
            (AttackArea.SingleAlly, false, false, true, true, false) => SkillTarget.SingleAllyAliveExcludingSelf,
            (AttackArea.SingleAlly, false, false, false, true, true) => SkillTarget.SingleAllyFaintedExcludingSelf,
            (AttackArea.AllParty, false, false, false, false, false) => SkillTarget.AllParty,
            (AttackArea.All, false, false, false, false, false) => SkillTarget.All,
            (AttackArea.None, false, false, false, false, false) => SkillTarget.None,
            (AttackArea.User, false, false, false, false, false) => SkillTarget.User,
            // RevivalMassage - onlyGroundedEnemies doesn't take effect since it's SingleAlly
            (AttackArea.SingleAlly, true, false, false, false, false) => SkillTarget.SingleAlly,
            // FrigidCoffin - onlyPlayersAlive doesn't take effect since it involves selecting an enemy, not a player
            (AttackArea.SingleEnemy, true, false, true, false, false) => SkillTarget.SingleEnemyGround,
            // ChargeUpPlus - onlyPlayersAlive doesn't take effect since the only way it would is if the user is somehow dead
            (AttackArea.AllParty, false, false, true, false, false) => SkillTarget.AllParty,
            _ => ThrowHelper.ThrowNotSupportedException<SkillTarget>(
                $"Invalid {nameof(RawTargetingParameters)}: {targetParameters}")
        };

        leaf.UsableBy = usabilityParameters switch
        {
            (false, false, false) => SkillUsability.AnyBug,
            (false, false, true) => SkillUsability.Moth,
            (false, true, false) => SkillUsability.Beetle,
            (false, true, true) => SkillUsability.BeetleAndMoth,
            (true, false, false) => SkillUsability.Bee,
            (true, false, true) => SkillUsability.BeeAndMoth,
            (true, true, false) => SkillUsability.BeeAndBeetle,
            (true, true, true) => SkillUsability.AnyBugWithAtLeastOneValidEnemyTarget,
        };
    }
}