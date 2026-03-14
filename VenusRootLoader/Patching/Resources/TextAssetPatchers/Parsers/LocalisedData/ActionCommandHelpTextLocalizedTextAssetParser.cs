using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers.LocalisedData;

internal sealed class ActionCommandHelpTextLocalizedTextAssetParser : ILocalizedTextAssetParser<ActionCommandHelpTextLeaf>
{
    public string GetTextAssetSerializedString(
        string subPath,
        int languageId,
        ActionCommandHelpTextLeaf helpTextLeaf) =>
        helpTextLeaf.HelpText[languageId];

    public void FromTextAssetSerializedString(
        string subPath,
        int languageId,
        string text,
        ActionCommandHelpTextLeaf helpTextLeaf) =>
        helpTextLeaf.HelpText[languageId] = text;
}