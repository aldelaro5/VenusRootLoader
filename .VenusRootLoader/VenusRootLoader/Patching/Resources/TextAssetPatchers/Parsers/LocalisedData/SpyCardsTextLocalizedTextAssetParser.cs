using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers.LocalisedData;

/// <inheritdoc/>
internal sealed class SpyCardsTextLocalizedTextAssetParser : ILocalizedTextAssetParser<SpyCardsTextLeaf>
{
    private readonly ILeavesRegistry<LanguageLeaf> _languageRegistry;

    public SpyCardsTextLocalizedTextAssetParser(ILeavesRegistry<LanguageLeaf> languageRegistry)
    {
        _languageRegistry = languageRegistry;
    }

    public string GetTextAssetSerializedString(string subPath, int languageId, SpyCardsTextLeaf leaf) =>
        leaf.LocalizedText[_languageRegistry.GetByGameId(languageId)];

    public void FromTextAssetSerializedString(string subPath, int languageId, string text, SpyCardsTextLeaf leaf) =>
        leaf.LocalizedText[_languageRegistry.GetByGameId(languageId)] = text;
}