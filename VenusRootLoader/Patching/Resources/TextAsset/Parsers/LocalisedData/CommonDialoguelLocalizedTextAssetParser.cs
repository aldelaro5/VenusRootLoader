using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Patching.Resources.TextAsset.Parsers.LocalisedData;

internal sealed class CommonDialoguelLocalizedTextAssetParser : ILocalizedTextAssetParser<CommonDialogueLeaf>
{
    public string GetTextAssetSerializedString(string subPath, int languageId, CommonDialogueLeaf leaf) =>
        leaf.LocalizedText[languageId];

    public void FromTextAssetSerializedString(string subPath, int languageId, string text, CommonDialogueLeaf leaf) =>
        leaf.LocalizedText[languageId] = text;
}