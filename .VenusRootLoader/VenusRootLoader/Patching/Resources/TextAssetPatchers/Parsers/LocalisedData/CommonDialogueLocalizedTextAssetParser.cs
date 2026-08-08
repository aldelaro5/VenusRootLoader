using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers.LocalisedData;

/// <inheritdoc/>
internal sealed class CommonDialogueLocalizedTextAssetParser : ILocalizedTextAssetParser<CommonDialogueLeaf>
{
    private readonly ILeavesRegistry<LanguageLeaf> _languageRegistry;

    public CommonDialogueLocalizedTextAssetParser(ILeavesRegistry<LanguageLeaf> languageRegistry)
    {
        _languageRegistry = languageRegistry;
    }

    public string GetTextAssetSerializedString(string subPath, int languageId, CommonDialogueLeaf leaf) =>
        leaf.LocalizedText[_languageRegistry.GetByGameId(languageId)];

    public void FromTextAssetSerializedString(string subPath, int languageId, string text, CommonDialogueLeaf leaf) =>
        leaf.LocalizedText[_languageRegistry.GetByGameId(languageId)] = text;
}