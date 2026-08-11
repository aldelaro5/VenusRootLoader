using CommunityToolkit.Diagnostics;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers.LocalisedData;

// TODO: Look into the bad performance, see if we can patch the game so it doesn't lag as hard.

/// <inheritdoc/>
internal sealed class LoreBookLocalizedTextAssetParser : ILocalizedTextAssetParser<LoreBookLeaf>
{
    private readonly ILeavesRegistry<LanguageLeaf> _languageRegistry;

    public LoreBookLocalizedTextAssetParser(ILeavesRegistry<LanguageLeaf> languageRegistry)
    {
        _languageRegistry = languageRegistry;
    }

    private const string LoreBookTextSubpath = "LoreText";
    private const string FortuneTeller1Subpath = "FortuneTeller1";

    public string GetTextAssetSerializedString(string subPath, int languageId, LoreBookLeaf leaf)
    {
        return subPath switch
        {
            LoreBookTextSubpath => $"{leaf.LocalizedData[_languageRegistry.GetByGameId(languageId)].Title}@" +
                                   $"{leaf.LocalizedData[_languageRegistry.GetByGameId(languageId)].Content}",
            FortuneTeller1Subpath => leaf.LocalizedData[_languageRegistry.GetByGameId(languageId)].FortuneTellerHint,
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<string>(nameof(subPath))
        };
    }

    public void FromTextAssetSerializedString(string subPath, int languageId, string text, LoreBookLeaf leaf)
    {
        if (subPath == FortuneTeller1Subpath)
        {
            leaf.LocalizedData[_languageRegistry.GetByGameId(languageId)].FortuneTellerHint = text;
            return;
        }

        if (subPath != LoreBookTextSubpath)
            ThrowHelper.ThrowArgumentOutOfRangeException<string>(nameof(subPath));

        string[] fields = text.Split(StringUtils.AtSymbolSplitDelimiter);
        leaf.LocalizedData[_languageRegistry.GetByGameId(languageId)].Title = fields[0];
        leaf.LocalizedData[_languageRegistry.GetByGameId(languageId)].Content = fields[1];
    }
}