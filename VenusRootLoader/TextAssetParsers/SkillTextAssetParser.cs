using System.Text;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Extensions;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.TextAssetParsers;

internal class SkillTextAssetParser : ITextAssetParser<SkillLeaf>
{
    public string GetTextAssetSerializedString(string subPath, SkillLeaf leaf)
    {
        StringBuilder sb = new();

        sb.Append(leaf.Cost);
        sb.Append('@');
        sb.Append(leaf.Target.ToString());
        sb.Append('@');
        sb.Append(leaf.UsableByBee);
        sb.Append('@');
        sb.Append(leaf.UsableByBeetle);
        sb.Append('@');
        sb.Append(leaf.UsableByMoth);
        sb.Append('@');
        sb.Append(leaf.TargetsOnlyGroundedEnemies);
        sb.Append('@');
        sb.Append(leaf.TargetsOnlyFrontEnemy);
        sb.Append('@');
        sb.Append(leaf.ActionCommandId);
        sb.Append('@');
        sb.Append(leaf.TargetsAliveOnly);
        sb.Append('@');
        sb.Append(leaf.IsUserExcludedFromTarget);
        sb.Append('@');
        sb.Append(leaf.TargetFaintedOnly);

        return sb.ToString();
    }

    public void FromTextAssetSerializedString(string subPath, string text, SkillLeaf leaf)
    {
        string[] fields = text.Split(StringUtils.AtSymbolSplitDelimiter);

        leaf.Cost = int.Parse(fields[0]);
        leaf.Target = Enum.Parse<BattleControl.AttackArea>(fields[1]);
        leaf.UsableByBee = bool.Parse(fields[2]);
        leaf.UsableByBeetle = bool.Parse(fields[3]);
        leaf.UsableByMoth = bool.Parse(fields[4]);
        leaf.TargetsOnlyGroundedEnemies = bool.Parse(fields[5]);
        leaf.TargetsOnlyFrontEnemy = bool.Parse(fields[6]);
        leaf.ActionCommandId = int.Parse(fields[7]);
        leaf.TargetsAliveOnly = bool.Parse(fields[8]);
        leaf.IsUserExcludedFromTarget = bool.Parse(fields[9]);
        leaf.TargetFaintedOnly = bool.Parse(fields[10]);
    }
}