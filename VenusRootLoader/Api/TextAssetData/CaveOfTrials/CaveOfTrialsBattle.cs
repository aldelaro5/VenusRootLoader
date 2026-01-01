using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Api.TextAssetData.CaveOfTrials;

public sealed class CaveOfTrialsBattle : ITextAssetSerializable
{
    public List<int> EnemyIdsInBattle { get; } = new();

    string ITextAssetSerializable.GetTextAssetSerializedString() => string.Join(",", EnemyIdsInBattle);

    void ITextAssetSerializable.FromTextAssetSerializedString(string text)
    {
        string[] enemyIds = text.Split(StringUtils.CommaSplitDelimiter);

        EnemyIdsInBattle.Clear();
        foreach (string enemyId in enemyIds)
            EnemyIdsInBattle.Add(int.Parse(enemyId));
    }
}