using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.TextAsset.Parsers.GlobalData;

internal sealed class CaveOfTrialsBattleTextAssetParser : ITextAssetParser<CaveOfTrialsBattleLeaf>
{
    public string GetTextAssetSerializedString(string subPath, CaveOfTrialsBattleLeaf leaf)
        => string.Join(",", leaf.EnemyIdsInBattle);

    public void FromTextAssetSerializedString(string subPath, string text, CaveOfTrialsBattleLeaf leaf)
    {
        string[] enemyIds = text.Split(StringUtils.CommaSplitDelimiter);

        leaf.EnemyIdsInBattle.Clear();
        foreach (string enemyId in enemyIds)
            leaf.EnemyIdsInBattle.Add(int.Parse(enemyId));
    }
}