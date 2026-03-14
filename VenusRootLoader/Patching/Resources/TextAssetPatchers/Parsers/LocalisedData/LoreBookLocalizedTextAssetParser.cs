using CommunityToolkit.Diagnostics;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers.LocalisedData;

internal sealed class LoreBookLocalizedTextAssetParser : ILocalizedTextAssetParser<LoreBookLeaf>
{
    private const string LoreBookTextSubpath = "LoreText";
    private const string FortuneTeller1Subpath = "FortuneTeller1";

    public string GetTextAssetSerializedString(string subPath, int languageId, LoreBookLeaf leaf)
    {
        return subPath switch
        {
            LoreBookTextSubpath => $"{leaf.LocalizedData[languageId].Title}@{leaf.LocalizedData[languageId].Content}",
            FortuneTeller1Subpath => leaf.LocalizedData[languageId].FortuneTellerHint,
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<string>(nameof(subPath))
        };
    }

    public void FromTextAssetSerializedString(string subPath, int languageId, string text, LoreBookLeaf leaf)
    {
        if (subPath == FortuneTeller1Subpath)
        {
            leaf.LocalizedData[languageId].FortuneTellerHint = text;
            return;
        }

        if (subPath != LoreBookTextSubpath)
            ThrowHelper.ThrowArgumentOutOfRangeException<string>(nameof(subPath));

        string[] fields = text.Split(StringUtils.AtSymbolSplitDelimiter);
        leaf.LocalizedData[languageId].Title = fields[0];
        leaf.LocalizedData[languageId].Content = fields[1];
    }
}