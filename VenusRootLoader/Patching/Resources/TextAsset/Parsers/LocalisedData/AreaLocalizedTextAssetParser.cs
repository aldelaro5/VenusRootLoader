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
        AreaLeaf.AreaLanguageData? languageData = null;
        if (leaf.LanguageData.Count == 0)
            languageData = Activator.CreateInstance<AreaLeaf.AreaLanguageData>();

        if (leaf.LanguageData.TryGetValue(languageId, out AreaLeaf.AreaLanguageData value))
            languageData = value;

        if (languageData == null)
        {
            int firstLanguage = leaf.LanguageData.Keys.Min();
            languageData = leaf.LanguageData[firstLanguage];
        }
        
        if (subPath.Equals(NameSubpath, StringComparison.InvariantCultureIgnoreCase))
            return languageData.Name;

        return subPath.Equals(DescriptionSubpath, StringComparison.InvariantCultureIgnoreCase)
            ? string.Join("{", languageData.PaginatedDescription)
            : ThrowHelper.ThrowInvalidOperationException<string>($"This parser doesn't support the subPath {subPath}");
    }

    public void FromTextAssetSerializedString(string subPath, int languageId, string text, AreaLeaf leaf)
    {
        if (!leaf.LanguageData.ContainsKey(languageId))
            leaf.LanguageData[languageId] = new();

        if (subPath.Equals(NameSubpath, StringComparison.InvariantCultureIgnoreCase))
        {
            leaf.LanguageData[languageId].Name = text;
        }
        else if (subPath.Equals(DescriptionSubpath, StringComparison.InvariantCultureIgnoreCase))
        {
            string[] pages = text.Split(StringUtils.OpeningBraceSplitDelimiter);
            foreach (string page in pages)
                leaf.LanguageData[languageId].PaginatedDescription.Add(page);
        }
        else
        {
            ThrowHelper.ThrowInvalidOperationException($"This parser doesn't support the subPath {subPath}");
        }
    }
}