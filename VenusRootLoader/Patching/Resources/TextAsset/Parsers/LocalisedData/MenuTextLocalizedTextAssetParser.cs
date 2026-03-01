using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Patching.Resources.TextAsset.Parsers.LocalisedData;

internal sealed class MenuTextLocalizedTextAssetParser : ILocalizedTextAssetParser<MenuTextLeaf>
{
    public string GetTextAssetSerializedString(string subPath, int languageId, MenuTextLeaf leaf) =>
        leaf.LocalizedText[languageId];

    public void FromTextAssetSerializedString(string subPath, int languageId, string text, MenuTextLeaf leaf) =>
        leaf.LocalizedText[languageId] = text;
}