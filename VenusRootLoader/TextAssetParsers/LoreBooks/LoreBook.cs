using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.TextAssetParsers.LoreBooks;

internal sealed class LoreBook : ITextAssetSerializable
{
    internal string Title { get; set; } = "";
    internal string Content { get; set; } = "";

    string ITextAssetSerializable.GetTextAssetSerializedString() => $"{Title}@{Content}";

    void ITextAssetSerializable.FromTextAssetSerializedString(string text)
    {
        string[] fields = text.Split(StringUtils.AtSymbolSplitDelimiter);

        Title = fields[0];
        Content = fields[1];
    }
}