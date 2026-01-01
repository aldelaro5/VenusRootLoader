using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Api.TextAssetData.Quests;

public sealed class QuestData : ITextAssetSerializable
{
    public int BoundTakenFlagId { get; set; }
    internal int EnemyPortraitsSpriteIndexForIcon { get; set; }
    public int Difficulty { get; set; }

    string ITextAssetSerializable.GetTextAssetSerializedString() =>
        $"{BoundTakenFlagId}@{EnemyPortraitsSpriteIndexForIcon}@{Difficulty}";

    void ITextAssetSerializable.FromTextAssetSerializedString(string text)
    {
        string[] fields = text.Split(StringUtils.AtSymbolSplitDelimiter);

        BoundTakenFlagId = int.Parse(fields[0]);
        EnemyPortraitsSpriteIndexForIcon = int.Parse(fields[1]);
        Difficulty = int.Parse(fields[2]);
    }
}