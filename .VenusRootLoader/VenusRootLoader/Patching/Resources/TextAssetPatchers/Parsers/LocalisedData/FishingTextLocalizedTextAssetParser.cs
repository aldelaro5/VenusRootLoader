using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers.LocalisedData;

/// <inheritdoc/>
internal sealed class FishingTextLocalizedTextAssetParser : ILocalizedTextAssetParser<FishingTextLeaf>
{
    private readonly ILeavesRegistry<LanguageLeaf> _languageRegistry;

    public FishingTextLocalizedTextAssetParser(ILeavesRegistry<LanguageLeaf> languageRegistry)
    {
        _languageRegistry = languageRegistry;
    }

    public string GetTextAssetSerializedString(string subPath, int languageId, FishingTextLeaf leaf) =>
        leaf.LocalizedText[_languageRegistry.GetByGameId(languageId)];

    public void FromTextAssetSerializedString(string subPath, int languageId, string text, FishingTextLeaf leaf) =>
        leaf.LocalizedText[_languageRegistry.GetByGameId(languageId)] = text;
}