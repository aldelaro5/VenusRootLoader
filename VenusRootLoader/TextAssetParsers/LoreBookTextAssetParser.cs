using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.TextAssetParsers;

internal sealed class LoreBookTextAssetParser : ILocalizedTextAssetParser<LoreBookLeaf>
{
    public string GetTextAssetSerializedString(string subPath, int languageId, LoreBookLeaf leaf)
        => $"{leaf.Title[languageId]}@{leaf.Content[languageId]}";

    public void FromTextAssetSerializedString(string subPath, int languageId, string text, LoreBookLeaf leaf)
    {
        string[] fields = text.Split(StringUtils.AtSymbolSplitDelimiter);

        leaf.Title[languageId] = fields[0];
        leaf.Content[languageId] = fields[1];
    }
}