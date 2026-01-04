using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAsset;

namespace VenusRootLoader.TextAssetParsers;

internal sealed class MusicLocalizedTextAssetParser : ILocalizedTextAssetSerializable<MusicLeaf>
{
    public string GetTextAssetSerializedString(string subPath, int languageId, MusicLeaf leaf)
        => leaf.Title[languageId];

    public void FromTextAssetSerializedString(string subPath, int languageId, string text, MusicLeaf leaf)
        => leaf.Title[languageId] = text;
}