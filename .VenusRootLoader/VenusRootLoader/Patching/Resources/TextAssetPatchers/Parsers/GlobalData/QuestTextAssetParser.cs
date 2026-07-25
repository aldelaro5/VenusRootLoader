using CommunityToolkit.Diagnostics;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers.GlobalData;

/// <inheritdoc/>
internal sealed class QuestTextAssetParser : ITextAssetParser<QuestLeaf>
{
    private const string BoardDataSubPath = "BoardData";
    private const string QuestChecksSubPath = "QuestChecks";

    private readonly ILeavesRegistry<FlagLeaf> _flagsRegistry;
    private readonly ILeavesRegistry<AreaLeaf> _areasRegistry;

    public QuestTextAssetParser(ILeavesRegistry<FlagLeaf> flagsRegistry, ILeavesRegistry<AreaLeaf> areasRegistry)
    {
        _flagsRegistry = flagsRegistry;
        _areasRegistry = areasRegistry;
    }

    public string GetTextAssetSerializedString(string subPath, QuestLeaf leaf)
    {
        if (subPath.Equals(BoardDataSubPath, StringComparison.OrdinalIgnoreCase))
        {
            int enemyPortraitsSpriteIndex = ((IEnemyPortraitSprite)leaf).EnemyPortraitsSpriteIndex!.Value;
            return $"{leaf.TakenFlag?.GameId ?? -1}@{enemyPortraitsSpriteIndex}@{leaf.Difficulty}";
        }

        if (!subPath.Equals(QuestChecksSubPath, StringComparison.OrdinalIgnoreCase))
            return ThrowHelper.ThrowInvalidDataException<string>($"This parser doesn't support the subPath {subPath}");

        IEnumerable<string> requiredFlags = leaf.RequiredFlags.Select(b => b.GameId.ToString());
        IEnumerable<string> requiredSeenAreas = leaf.RequiredSeenAreas.Select(b => (-b.GameId).ToString());
        List<string> allRequirements = requiredFlags.Concat(requiredSeenAreas).ToList();
        return string.Join("@", allRequirements.Count == 0 ? ["0"] : allRequirements);
    }

    public void FromTextAssetSerializedString(string subPath, string text, QuestLeaf leaf)
    {
        if (subPath.Equals(BoardDataSubPath, StringComparison.OrdinalIgnoreCase))
        {
            string[] fieldsBoardData = text.Split(StringUtils.AtSymbolSplitDelimiter);

            int takenFlag = int.Parse(fieldsBoardData[0]);
            leaf.TakenFlag = takenFlag <= -1 ? null : new(_flagsRegistry.LeavesByGameIds[takenFlag]);
            ((IEnemyPortraitSprite)leaf).EnemyPortraitsSpriteIndex = int.Parse(fieldsBoardData[1]);
            leaf.Difficulty = int.Parse(fieldsBoardData[2]);
            return;
        }

        if (!subPath.Equals(QuestChecksSubPath, StringComparison.OrdinalIgnoreCase))
        {
            ThrowHelper.ThrowInvalidOperationException($"This parser doesn't support the subPath {subPath}");
            return;
        }

        string[] fieldsChecks = text.Split(StringUtils.AtSymbolSplitDelimiter);

        leaf.RequiredFlags.Clear();
        leaf.RequiredSeenAreas.Clear();
        for (int i = 0; i < fieldsChecks.Length; i++)
        {
            int check = int.Parse(fieldsChecks[i]);
            if (check == 0 && i == 0)
                break;

            if (check > 0)
                leaf.RequiredFlags.Add(new(_flagsRegistry.LeavesByGameIds[check]));
            else
                leaf.RequiredSeenAreas.Add(new(_areasRegistry.LeavesByGameIds[Math.Abs(check)]));
        }
    }
}