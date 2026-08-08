using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers.LocalisedData;

/// <inheritdoc/>
internal sealed class MusicLocalizedTextAssetParser : ILocalizedTextAssetParser<MusicLeaf>
{
    private readonly ILeavesRegistry<LanguageLeaf> _languageRegistry;

    public MusicLocalizedTextAssetParser(ILeavesRegistry<LanguageLeaf> languageRegistry)
    {
        _languageRegistry = languageRegistry;
    }

    public string GetTextAssetSerializedString(string subPath, int languageId, MusicLeaf leaf)
        => leaf.SamiraDisplayTitle[_languageRegistry.GetByGameId(languageId)];

    public void FromTextAssetSerializedString(string subPath, int languageId, string text, MusicLeaf leaf)
        => leaf.SamiraDisplayTitle[_languageRegistry.GetByGameId(languageId)] = text;
}