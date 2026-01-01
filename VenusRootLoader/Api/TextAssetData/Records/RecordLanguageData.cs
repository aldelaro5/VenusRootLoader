using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Api.TextAssetData.Records;

public sealed class RecordLanguageData : ITextAssetSerializable
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";

    string ITextAssetSerializable.GetTextAssetSerializedString() => $"{Name}@{Description}";

    void ITextAssetSerializable.FromTextAssetSerializedString(string text)
    {
        string[] fields = text.Split(StringUtils.AtSymbolSplitDelimiter);

        Name = fields[0];
        Description = fields[1];
    }
}