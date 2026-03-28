using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers.LocalisedData;

/// <inheritdoc/>
internal sealed class SpyCardsTextLocalizedTextAssetParser : ILocalizedTextAssetParser<SpyCardsTextLeaf>
{
    public string GetTextAssetSerializedString(string subPath, int languageId, SpyCardsTextLeaf leaf) =>
        leaf.LocalizedText[languageId];

    public void FromTextAssetSerializedString(string subPath, int languageId, string text, SpyCardsTextLeaf leaf) =>
        leaf.LocalizedText[languageId] = text;
}