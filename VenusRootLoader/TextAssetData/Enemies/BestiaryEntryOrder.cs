using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.TextAssetData.Enemies;

internal sealed class BestiaryEntryOrder : ITextAssetSerializable
{
    internal List<int> OrderedEnemyGameIds { get; } = new();

    string ITextAssetSerializable.GetTextAssetSerializedString() => string.Join("\n", OrderedEnemyGameIds);

    void ITextAssetSerializable.FromTextAssetSerializedString(string text)
    {
        string[] lines = text.Split(StringUtils.NewlineSplitDelimiter);
        OrderedEnemyGameIds.Clear();
        foreach (string line in lines)
            OrderedEnemyGameIds.Add(int.Parse(line));
    }
}