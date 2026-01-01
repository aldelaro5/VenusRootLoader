using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Api.TextAssetData.LoreBooks;

public sealed class LoreBook : ITextAssetSerializable
{
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";

    string ITextAssetSerializable.GetTextAssetSerializedString() => $"{Title}@{Content}";

    void ITextAssetSerializable.FromTextAssetSerializedString(string text)
    {
        string[] fields = text.Split(StringUtils.AtSymbolSplitDelimiter);

        Title = fields[0];
        Content = fields[1];
    }
}