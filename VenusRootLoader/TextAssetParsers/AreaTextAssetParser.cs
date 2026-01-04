using CommunityToolkit.Diagnostics;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.TextAssetParsers;

internal sealed class AreaTextAssetParser : ILocalizedTextAssetSerializable<AreaLeaf>
{
    private const string NameSubpath = "AreaNames";
    private const string DescriptionSubpath = "AreaDesc";

    public string GetTextAssetSerializedString(string subPath, int languageId, AreaLeaf leaf)
    {
        if (subPath.Equals(NameSubpath, StringComparison.InvariantCultureIgnoreCase))
            return leaf.Name[languageId];

        return subPath.Equals(DescriptionSubpath, StringComparison.InvariantCultureIgnoreCase)
            ? string.Join("{", leaf.PaginatedDescription[languageId])
            : ThrowHelper.ThrowInvalidOperationException<string>($"This parser doesn't support the subPath {subPath}");
    }

    public void FromTextAssetSerializedString(string subPath, int languageId, string text, AreaLeaf leaf)
    {
        if (subPath.Equals(NameSubpath, StringComparison.InvariantCultureIgnoreCase))
        {
            leaf.Name[languageId] = text;
        }
        else if (subPath.Equals(DescriptionSubpath, StringComparison.InvariantCultureIgnoreCase))
        {
            string[] pages = text.Split(StringUtils.OpeningBraceSplitDelimiter);

            leaf.PaginatedDescription[languageId] = new();
            foreach (string page in pages)
                leaf.PaginatedDescription[languageId].Add(page);
        }
        else
        {
            ThrowHelper.ThrowInvalidOperationException($"This parser doesn't support the subPath {subPath}");
        }
    }
}