using CommunityToolkit.Diagnostics;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.TextAssetParsers;

internal sealed class QuestTextAssetParser : ITextAssetSerializable<QuestLeaf>
{
    private const string BoardDataSubPath = "BoardData";
    private const string QuestChecksSubPath = "QuestChecks";

    public string GetTextAssetSerializedString(string subPath, QuestLeaf leaf)
    {
        if (subPath.Equals(BoardDataSubPath, StringComparison.OrdinalIgnoreCase))
            return $"{leaf.BoundTakenFlagId}@{leaf.EnemyPortraitsSpriteIndexForIcon}@{leaf.Difficulty}";
        return subPath.Equals(QuestChecksSubPath, StringComparison.OrdinalIgnoreCase)
            ? string.Join("@", leaf.RequiredFlagIds)
            : ThrowHelper.ThrowInvalidDataException<string>($"This parser doesn't support the subPath {subPath}");
    }

    public void FromTextAssetSerializedString(string subPath, string text, QuestLeaf leaf)
    {
        if (subPath.Equals(BoardDataSubPath, StringComparison.OrdinalIgnoreCase))
        {
            string[] fields = text.Split(StringUtils.AtSymbolSplitDelimiter);

            leaf.BoundTakenFlagId = int.Parse(fields[0]);
            leaf.EnemyPortraitsSpriteIndexForIcon = int.Parse(fields[1]);
            leaf.Difficulty = int.Parse(fields[2]);
        }
        else if (subPath.Equals(QuestChecksSubPath, StringComparison.OrdinalIgnoreCase))
        {
            string[] fields = text.Split(StringUtils.AtSymbolSplitDelimiter);

            leaf.RequiredFlagIds.Clear();
            foreach (string field in fields)
                leaf.RequiredFlagIds.Add(int.Parse(field));
        }
        else
        {
            ThrowHelper.ThrowInvalidOperationException($"This parser doesn't support the subPath {subPath}");
        }
    }
}