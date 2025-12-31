using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Api.TextAssetData.SpyCards;

public sealed class SpyCardOrder : ITextAssetSerializable
{
    public List<int> OrderedCardGameIds { get; } = new();

    string ITextAssetSerializable.GetTextAssetSerializedString() => string.Join("\n", OrderedCardGameIds);

    void ITextAssetSerializable.FromTextAssetSerializedString(string text)
    {
        string[] lines = text.Split(StringUtils.NewlineSplitDelimiter);
        OrderedCardGameIds.Clear();
        foreach (string line in lines)
            OrderedCardGameIds.Add(int.Parse(line));
    }
}