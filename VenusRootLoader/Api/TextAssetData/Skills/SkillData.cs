using System.Text;
using VenusRootLoader.Extensions;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Api.TextAssetData.Skills;

public class SkillData : ITextAssetSerializable
{
    public int Cost { get; set; }
    public BattleControl.AttackArea Target { get; set; }
    public bool UsableByBee { get; set; }
    public bool UsableByBeetle { get; set; }
    public bool UsableByMoth { get; set; }
    public bool TargetsOnlyGroundedEnemies { get; set; }
    public bool TargetsOnlyFrontEnemy { get; set; }
    public int ActionCommandId { get; set; }
    public bool TargetsAliveOnly { get; set; }
    public bool IsUserExcludedFromTarget { get; set; }
    public bool TargetFaintedOnly { get; set; }

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