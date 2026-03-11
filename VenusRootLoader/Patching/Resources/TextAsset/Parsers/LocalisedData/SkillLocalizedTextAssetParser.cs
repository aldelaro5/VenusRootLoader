using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.TextAsset.Parsers.LocalisedData;

internal sealed class SkillLocalizedTextAssetParser : ILocalizedTextAssetParser<SkillLeaf>
{
    public string GetTextAssetSerializedString(string subPath, int languageId, SkillLeaf leaf)
        => $"{leaf.LocalizedData[languageId].Name}@{leaf.LocalizedData[languageId].Description}";

    public void FromTextAssetSerializedString(string subPath, int languageId, string text, SkillLeaf leaf)
    {
        string[] fields = text.Split(StringUtils.AtSymbolSplitDelimiter);
        leaf.LocalizedData[languageId].Name = fields[0];
        leaf.LocalizedData[languageId].Description = fields[1];
    }
}