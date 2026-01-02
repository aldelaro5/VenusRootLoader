using System.Text;
using VenusRootLoader.Extensions;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.TextAssetData.Skills;

internal class SkillData : ITextAssetSerializable
{
    internal int Cost { get; set; }
    internal BattleControl.AttackArea Target { get; set; }
    internal bool UsableByBee { get; set; }
    internal bool UsableByBeetle { get; set; }
    internal bool UsableByMoth { get; set; }
    internal bool TargetsOnlyGroundedEnemies { get; set; }
    internal bool TargetsOnlyFrontEnemy { get; set; }
    internal int ActionCommandId { get; set; }
    internal bool TargetsAliveOnly { get; set; }
    internal bool IsUserExcludedFromTarget { get; set; }
    internal bool TargetFaintedOnly { get; set; }

    string ITextAssetSerializable.GetTextAssetSerializedString()
    {
        StringBuilder sb = new();

        sb.Append(Cost);
        sb.Append('@');
        sb.Append(Target.ToString());
        sb.Append('@');
        sb.Append(UsableByBee);
        sb.Append('@');
        sb.Append(UsableByBeetle);
        sb.Append('@');
        sb.Append(UsableByMoth);
        sb.Append('@');
        sb.Append(TargetsOnlyGroundedEnemies);
        sb.Append('@');
        sb.Append(TargetsOnlyFrontEnemy);
        sb.Append('@');
        sb.Append(ActionCommandId);
        sb.Append('@');
        sb.Append(TargetsAliveOnly);
        sb.Append('@');
        sb.Append(IsUserExcludedFromTarget);
        sb.Append('@');
        sb.Append(TargetFaintedOnly);

        return sb.ToString();
    }

    void ITextAssetSerializable.FromTextAssetSerializedString(string text)
    {
        string[] fields = text.Split(StringUtils.AtSymbolSplitDelimiter);

        Cost = int.Parse(fields[0]);
        Target = Enum.Parse<BattleControl.AttackArea>(fields[1]);
        UsableByBee = bool.Parse(fields[2]);
        UsableByBeetle = bool.Parse(fields[3]);
        UsableByMoth = bool.Parse(fields[4]);
        TargetsOnlyGroundedEnemies = bool.Parse(fields[5]);
        TargetsOnlyFrontEnemy = bool.Parse(fields[6]);
        ActionCommandId = int.Parse(fields[7]);
        TargetsAliveOnly = bool.Parse(fields[8]);
        IsUserExcludedFromTarget = bool.Parse(fields[9]);
        TargetFaintedOnly = bool.Parse(fields[10]);
    }
}