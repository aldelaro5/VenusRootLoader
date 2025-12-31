using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Api.TextAssetData.Medals;

public sealed class MedalOrder : ITextAssetSerializable
{
    public List<int> OrderedMedalGameIds { get; } = new();

    string ITextAssetSerializable.GetTextAssetSerializedString() => string.Join("\n", OrderedMedalGameIds);

    void ITextAssetSerializable.FromTextAssetSerializedString(string text)
    {
        string[] lines = text.Split(StringUtils.NewlineSplitDelimiter);
        OrderedMedalGameIds.Clear();
        foreach (string line in lines)
            OrderedMedalGameIds.Add(int.Parse(line));
    }
}