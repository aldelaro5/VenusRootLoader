using CommunityToolkit.Diagnostics;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers.LocalisedData;

/// <inheritdoc/>
internal sealed class AreaLocalizedTextAssetParser : ILocalizedTextAssetParser<AreaLeaf>
{
    private const string NameSubpath = "AreaNames";
    private const string DescriptionSubpath = "AreaDesc";

    private readonly ILeavesRegistry<LanguageLeaf> _languageRegistry;

    public AreaLocalizedTextAssetParser(ILeavesRegistry<LanguageLeaf> languageRegistry)
    {
        _languageRegistry = languageRegistry;
    }

    public string GetTextAssetSerializedString(string subPath, int languageId, AreaLeaf leaf)
    {
        if (subPath.Equals(NameSubpath, StringComparison.InvariantCultureIgnoreCase))
            return leaf.LocalizedData[_languageRegistry.GetByGameId(languageId)].Name;

        return subPath.Equals(DescriptionSubpath, StringComparison.InvariantCultureIgnoreCase)
            ? string.Join("{", leaf.LocalizedData[_languageRegistry.GetByGameId(languageId)].PaginatedDescription)
            : ThrowHelper.ThrowInvalidOperationException<string>($"This parser doesn't support the subPath {subPath}");
    }

    public void FromTextAssetSerializedString(string subPath, int languageId, string text, AreaLeaf leaf)
    {
        if (!leaf.LocalizedData.ContainsKey(_languageRegistry.GetByGameId(languageId)))
            leaf.LocalizedData[_languageRegistry.GetByGameId(languageId)] = new();

        if (subPath.Equals(NameSubpath, StringComparison.InvariantCultureIgnoreCase))
        {
            leaf.LocalizedData[_languageRegistry.GetByGameId(languageId)].Name = text;
        }
        else if (subPath.Equals(DescriptionSubpath, StringComparison.InvariantCultureIgnoreCase))
        {
            string[] pages = text.Split(StringUtils.OpeningBraceSplitDelimiter);
            foreach (string page in pages)
                leaf.LocalizedData[_languageRegistry.GetByGameId(languageId)].PaginatedDescription.Add(page);
        }
        else
        {
            ThrowHelper.ThrowInvalidOperationException($"This parser doesn't support the subPath {subPath}");
        }
    }
}