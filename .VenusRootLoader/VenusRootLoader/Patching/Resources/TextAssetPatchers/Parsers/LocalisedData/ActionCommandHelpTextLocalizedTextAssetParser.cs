using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers.LocalisedData;

/// <inheritdoc/>
internal sealed class
    ActionCommandHelpTextLocalizedTextAssetParser : ILocalizedTextAssetParser<SkillActionCommandHelpTextLeaf>
{
    private readonly ILeavesRegistry<LanguageLeaf> _languageRegistry;

    public ActionCommandHelpTextLocalizedTextAssetParser(ILeavesRegistry<LanguageLeaf> languageRegistry)
    {
        _languageRegistry = languageRegistry;
    }

    public string GetTextAssetSerializedString(
        string subPath,
        int languageId,
        SkillActionCommandHelpTextLeaf helpTextLeaf) =>
        helpTextLeaf.HelpText[_languageRegistry.GetByGameId(languageId)];

    public void FromTextAssetSerializedString(
        string subPath,
        int languageId,
        string text,
        SkillActionCommandHelpTextLeaf helpTextLeaf) =>
        helpTextLeaf.HelpText[_languageRegistry.GetByGameId(languageId)] = text;
}