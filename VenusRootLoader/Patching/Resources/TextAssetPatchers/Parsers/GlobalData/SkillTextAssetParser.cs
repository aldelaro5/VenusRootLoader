using CommunityToolkit.Diagnostics;
using System.Text;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Extensions;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;
using static BattleControl;
using static VenusRootLoader.Api.Leaves.SkillLeaf;
using RawUsabilityParameters = (bool usableByBee, bool usableByBeetle, bool usableByMoth);
using RawTargetingParameters = (BattleControl.AttackArea attackAtrea, bool onlyGroundedEnemies, bool onlyFrontEnemy,
    bool onlyPlayersAlive, bool excludeSelf, bool onlyPlayersFainted);

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers.GlobalData;

internal class SkillTextAssetParser : ITextAssetParser<SkillLeaf>
{
    private readonly ILeavesRegistry<ActionCommandHelpTextLeaf> _actionCommandHelpTextsRegistry;

    public SkillTextAssetParser(ILeavesRegistry<ActionCommandHelpTextLeaf> actionCommandHelpTextsRegistry)
    {
        _actionCommandHelpTextsRegistry = actionCommandHelpTextsRegistry;
    }

    public string GetTextAssetSerializedString(string subPath, SkillLeaf value)
    {
        RawTargetingParameters targetingParameters = value.Target switch
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
                $"Invalid {nameof(SkillTarget)}: {value.Target}")
        };

        RawUsabilityParameters usability = value.UsableBy switch
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
                $"Invalid {nameof(SkillUsability)}: {value.UsableBy}")
        };

        switch (value.NamedId)
        {
            case nameof(MainManager.Skills.RevivalMassage) when value.Target == SkillTarget.SingleAlly:
                targetingParameters.onlyGroundedEnemies = true;
                break;
            case nameof(MainManager.Skills.FrigidCoffin) when value.Target == SkillTarget.SingleEnemyGround:
            case nameof(MainManager.Skills.ChargeUpPlus) when value.Target == SkillTarget.AllParty:
                targetingParameters.onlyPlayersAlive = true;
                break;
        }

        StringBuilder sb = new();

        sb.Append(value.Cost * (value.CostResource == SkillCostResource.Tp ? 1 : -1));
        sb.Append('@');
        sb.Append(targetingParameters.attackAtrea.ToString());
        sb.Append('@');
        sb.Append(CamelCaseBoolIfNeeded(usability.usableByBee, value.OriginalBoolCasing, false));
        sb.Append('@');
        sb.Append(CamelCaseBoolIfNeeded(usability.usableByBeetle, value.OriginalBoolCasing, false));
        sb.Append('@');
        sb.Append(CamelCaseBoolIfNeeded(usability.usableByMoth, value.OriginalBoolCasing, false));
        sb.Append('@');
        sb.Append(CamelCaseBoolIfNeeded(targetingParameters.onlyGroundedEnemies, value.OriginalBoolCasing, false));
        sb.Append('@');
        sb.Append(CamelCaseBoolIfNeeded(targetingParameters.onlyFrontEnemy, value.OriginalBoolCasing, false));
        sb.Append('@');
        sb.Append(value.ActionCommandHelpText?.GameId ?? -1);
        sb.Append('@');
        sb.Append(CamelCaseBoolIfNeeded(targetingParameters.onlyPlayersAlive, value.OriginalBoolCasing, false));
        sb.Append('@');
        sb.Append(CamelCaseBoolIfNeeded(targetingParameters.excludeSelf, value.OriginalBoolCasing, true));
        sb.Append('@');
        sb.Append(CamelCaseBoolIfNeeded(targetingParameters.onlyPlayersFainted, value.OriginalBoolCasing, true));

        return sb.ToString();
    }

    public void FromTextAssetSerializedString(string subPath, string text, SkillLeaf value)
    {
        string[] fields = text.Split(StringUtils.AtSymbolSplitDelimiter);
        string usableByBeeString = fields[2];
        if (char.IsLower(usableByBeeString[0]))
        {
            value.OriginalBoolCasing = BoolCasing.AllCamelCase;
        }
        else
        {
            string onlyPlayersFaintedString = fields[10];
            value.OriginalBoolCasing = char.IsLower(onlyPlayersFaintedString[0])
                ? BoolCasing.PascalCaseWithLastTwoCamelCase
                : BoolCasing.AllPascalCase;
        }

        RawUsabilityParameters usabilityParameters =
            (bool.Parse(fields[2]), bool.Parse(fields[3]), bool.Parse(fields[4]));
        RawTargetingParameters targetParameters = (Enum.Parse<AttackArea>(fields[1]), bool.Parse(fields[5]),
            bool.Parse(fields[6]), bool.Parse(fields[8]),
            bool.Parse(fields[9]), bool.Parse(fields[10]));

        int cost = int.Parse(fields[0]);
        value.CostResource = cost >= 0 ? SkillCostResource.Tp : SkillCostResource.Hp;
        value.Cost = Math.Abs(cost);
        int actionCommandHelpTextGameId = int.Parse(fields[7]);
        value.ActionCommandHelpText = actionCommandHelpTextGameId > -1
            ? new(_actionCommandHelpTextsRegistry.LeavesByGameIds[actionCommandHelpTextGameId])
            : null;

        value.Target = targetParameters switch
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

        value.UsableBy = usabilityParameters switch
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

    private static string CamelCaseBoolIfNeeded(bool value, BoolCasing casing, bool isAmongLastTwo) =>
        casing switch
        {
            BoolCasing.AllPascalCase => value.ToString(),
            BoolCasing.AllCamelCase => value.ToString().ToLowerInvariant(),
            BoolCasing.PascalCaseWithLastTwoCamelCase => isAmongLastTwo
                ? value.ToString().ToLowerInvariant()
                : value.ToString(),
            _ => ThrowHelper.ThrowArgumentNullException<string>(nameof(casing))
        };
}