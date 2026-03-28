using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers.LocalisedData;

/// <inheritdoc/>
internal sealed class CommonDialogueLocalizedTextAssetParser : ILocalizedTextAssetParser<CommonDialogueLeaf>
{
    public string GetTextAssetSerializedString(string subPath, int languageId, CommonDialogueLeaf leaf) =>
        leaf.LocalizedText[languageId];

    public void FromTextAssetSerializedString(string subPath, int languageId, string text, CommonDialogueLeaf leaf) =>
        leaf.LocalizedText[languageId] = text;
}