using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.TextAssetParsers;

internal sealed class SkillLocalizedTextAssetParser : ILocalizedTextAssetSerializable<SkillLeaf>
{
    public string GetTextAssetSerializedString(string subPath, int languageId, SkillLeaf leaf)
        => $"{leaf.Name[languageId]}@{leaf.Description[languageId]}";

    public void FromTextAssetSerializedString(string subPath, int languageId, string text, SkillLeaf leaf)
    {
        string[] fields = text.Split(StringUtils.AtSymbolSplitDelimiter);

        leaf.Name[languageId] = fields[0];
        leaf.Description[languageId] = fields[1];
    }
}