using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.TextAssetData.SpyCards;

internal sealed class SpyCardOrder : ITextAssetSerializable
{
    internal List<int> OrderedCardGameIds { get; } = new();

    string ITextAssetSerializable.GetTextAssetSerializedString() => string.Join("\n", OrderedCardGameIds);

    void ITextAssetSerializable.FromTextAssetSerializedString(string text)
    {
        string[] lines = text.Split(StringUtils.NewlineSplitDelimiter);
        OrderedCardGameIds.Clear();
        foreach (string line in lines)
            OrderedCardGameIds.Add(int.Parse(line));
    }
}