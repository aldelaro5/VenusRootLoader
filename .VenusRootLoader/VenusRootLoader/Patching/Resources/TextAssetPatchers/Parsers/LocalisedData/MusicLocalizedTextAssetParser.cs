using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers.LocalisedData;

/// <inheritdoc/>
internal sealed class MusicLocalizedTextAssetParser : ILocalizedTextAssetParser<MusicLeaf>
{
    public string GetTextAssetSerializedString(string subPath, int languageId, MusicLeaf leaf)
        => leaf.SamiraDisplayTitle[languageId];

    public void FromTextAssetSerializedString(string subPath, int languageId, string text, MusicLeaf leaf)
        => leaf.SamiraDisplayTitle[languageId] = text;
}