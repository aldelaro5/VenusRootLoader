using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.TextAssetParsers.Discoveries;

internal sealed class DiscoveryData : ITextAssetSerializable
{
    internal int DiscoveryEntryGameId { get; set; }
    internal int EnemyPortraitsSpriteIndex { get; set; }

    string ITextAssetSerializable.GetTextAssetSerializedString() =>
        $"{DiscoveryEntryGameId},{EnemyPortraitsSpriteIndex}";

    void ITextAssetSerializable.FromTextAssetSerializedString(string text)
    {
        string[] fields = text.Split(StringUtils.CommaSplitDelimiter);
        DiscoveryEntryGameId = int.Parse(fields[0]);
        EnemyPortraitsSpriteIndex = int.Parse(fields[1]);
    }
}