using CommunityToolkit.Diagnostics;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.TextAsset.Parsers.LocalisedData;

internal sealed class AreaLocalizedTextAssetParser : ILocalizedTextAssetParser<AreaLeaf>
{
    private const string NameSubpath = "AreaNames";
    private const string DescriptionSubpath = "AreaDesc";

    public string GetTextAssetSerializedString(string subPath, int languageId, AreaLeaf leaf)
    {
        if (subPath.Equals(NameSubpath, StringComparison.InvariantCultureIgnoreCase))
            return leaf.LocalizedData[languageId].Name;

        return subPath.Equals(DescriptionSubpath, StringComparison.InvariantCultureIgnoreCase)
            ? string.Join("{", leaf.LocalizedData[languageId].PaginatedDescription)
            : ThrowHelper.ThrowInvalidOperationException<string>($"This parser doesn't support the subPath {subPath}");
    }

    public void FromTextAssetSerializedString(string subPath, int languageId, string text, AreaLeaf leaf)
    {
        if (!leaf.LocalizedData.ContainsKey(languageId))
            leaf.LocalizedData[languageId] = new();

        if (subPath.Equals(NameSubpath, StringComparison.InvariantCultureIgnoreCase))
        {
            leaf.LocalizedData[languageId].Name = text;
        }
        else if (subPath.Equals(DescriptionSubpath, StringComparison.InvariantCultureIgnoreCase))
        {
            string[] pages = text.Split(StringUtils.OpeningBraceSplitDelimiter);
            foreach (string page in pages)
                leaf.LocalizedData[languageId].PaginatedDescription.Add(page);
        }
        else
        {
            ThrowHelper.ThrowInvalidOperationException($"This parser doesn't support the subPath {subPath}");
        }
    }
}