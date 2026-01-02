using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.TextAssetParsers.Records;

internal sealed class RecordData : ITextAssetSerializable
{
    internal int RecordEntryGameId { get; set; }
    internal int EnemyPortraitsSpriteIndex { get; set; }

    string ITextAssetSerializable.GetTextAssetSerializedString() =>
        $"{RecordEntryGameId},{EnemyPortraitsSpriteIndex}";

    void ITextAssetSerializable.FromTextAssetSerializedString(string text)
    {
        string[] fields = text.Split(StringUtils.CommaSplitDelimiter);

        RecordEntryGameId = int.Parse(fields[0]);
        EnemyPortraitsSpriteIndex = int.Parse(fields[1]);
    }
}