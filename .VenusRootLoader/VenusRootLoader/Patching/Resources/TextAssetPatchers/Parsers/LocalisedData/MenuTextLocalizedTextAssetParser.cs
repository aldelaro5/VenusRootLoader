using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers.LocalisedData;

/// <inheritdoc/>
internal sealed class MenuTextLocalizedTextAssetParser : ILocalizedTextAssetParser<MenuTextLeaf>
{
    private readonly ILeavesRegistry<LanguageLeaf> _languageRegistry;

    public MenuTextLocalizedTextAssetParser(ILeavesRegistry<LanguageLeaf> languageRegistry)
    {
        _languageRegistry = languageRegistry;
    }

    public string GetTextAssetSerializedString(string subPath, int languageId, MenuTextLeaf leaf) =>
        leaf.LocalizedText[_languageRegistry.GetByGameId(languageId)];

    public void FromTextAssetSerializedString(string subPath, int languageId, string text, MenuTextLeaf leaf) =>
        leaf.LocalizedText[_languageRegistry.GetByGameId(languageId)] = text;
}