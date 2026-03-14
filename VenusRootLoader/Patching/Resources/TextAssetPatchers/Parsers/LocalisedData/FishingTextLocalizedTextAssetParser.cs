using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers.LocalisedData;

internal sealed class FishingTextLocalizedTextAssetParser : ILocalizedTextAssetParser<FishingTextLeaf>
{
    public string GetTextAssetSerializedString(string subPath, int languageId, FishingTextLeaf leaf) =>
        leaf.LocalizedText[languageId];

    public void FromTextAssetSerializedString(string subPath, int languageId, string text, FishingTextLeaf leaf) =>
        leaf.LocalizedText[languageId] = text;
}