using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Patching.Resources.TextAsset.Parsers.LocalisedData;

internal sealed class MusicLocalizedTextAssetParser : ILocalizedTextAssetParser<MusicLeaf>
{
    public string GetTextAssetSerializedString(string subPath, int languageId, MusicLeaf leaf)
        => leaf.Title[languageId];

    public void FromTextAssetSerializedString(string subPath, int languageId, string text, MusicLeaf leaf)
        => leaf.Title[languageId] = text;
}