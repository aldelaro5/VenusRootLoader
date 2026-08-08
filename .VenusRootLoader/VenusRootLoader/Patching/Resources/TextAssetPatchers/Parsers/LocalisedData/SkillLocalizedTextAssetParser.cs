using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers.LocalisedData;

/// <inheritdoc/>
internal sealed class SkillLocalizedTextAssetParser : ILocalizedTextAssetParser<SkillLeaf>
{
    private readonly ILeavesRegistry<LanguageLeaf> _languageRegistry;

    public SkillLocalizedTextAssetParser(ILeavesRegistry<LanguageLeaf> languageRegistry)
    {
        _languageRegistry = languageRegistry;
    }

    public string GetTextAssetSerializedString(string subPath, int languageId, SkillLeaf leaf)
        => $"{leaf.LocalizedData[_languageRegistry.GetByGameId(languageId)].Name}@" +
           $"{leaf.LocalizedData[_languageRegistry.GetByGameId(languageId)].Description}";

    public void FromTextAssetSerializedString(string subPath, int languageId, string text, SkillLeaf leaf)
    {
        string[] fields = text.Split(StringUtils.AtSymbolSplitDelimiter);
        leaf.LocalizedData[_languageRegistry.GetByGameId(languageId)].Name = fields[0];
        leaf.LocalizedData[_languageRegistry.GetByGameId(languageId)].Description = fields[1];
    }
}