using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.TextAssetData.Records;

internal sealed class RecordLanguageData : ITextAssetSerializable
{
    internal string Name { get; set; } = "";
    internal string Description { get; set; } = "";

    string ITextAssetSerializable.GetTextAssetSerializedString() => $"{Name}@{Description}";

    void ITextAssetSerializable.FromTextAssetSerializedString(string text)
    {
        string[] fields = text.Split(StringUtils.AtSymbolSplitDelimiter);

        Name = fields[0];
        Description = fields[1];
    }
}