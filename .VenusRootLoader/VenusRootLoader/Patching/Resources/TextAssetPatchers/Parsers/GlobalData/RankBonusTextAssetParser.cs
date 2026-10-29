using System.Text;
using VenusRootLoader.Api.Leaves.RankBonuses;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers.GlobalData;

/// <inheritdoc/>
internal sealed class RankBonusTextAssetParser : ITextAssetParser<RankBonusLeaf>
{
    public string GetTextAssetSerializedString(string subPath, RankBonusLeaf leaf)
    {
        StringBuilder sb = new();

        if (leaf is SkillLearnRankBonusLeaf skillLearnRankBonusLeaf)
        {
            // If multiple party members learns the same skill at once, we translate this by having multiple skill grant
            // bonuses, one for each party members that learns it. The game will show every message in a row.
            foreach (int skillLearnerTrueId in skillLearnRankBonusLeaf.GetSkillLearnerTrueIds())
            {
                if (sb.Length > 0)
                    sb.Append('\n');
                sb.Append(leaf.RankNeeded);
                sb.Append(',');
                sb.Append((int)leaf.InternalBonusType);
                sb.Append(',');
                sb.Append(skillLearnerTrueId);
                sb.Append(',');
                sb.Append(leaf.InternalSecondParameter);
                sb.Append(',');
                sb.Append(leaf.InternalThirdParameter);
            }
        }
        else
        {
            sb.Append(leaf.RankNeeded);
            sb.Append(',');
            sb.Append((int)leaf.InternalBonusType);
            sb.Append(',');
            sb.Append(leaf.InternalFirstParameter);
            sb.Append(',');
            sb.Append(leaf.InternalSecondParameter);
            sb.Append(',');
            sb.Append(leaf.InternalThirdParameter);
        }

        return sb.ToString();
    }

    public void FromTextAssetSerializedString(string subPath, string text, RankBonusLeaf leaf)
    {
        string[] split = text.Split(StringUtils.CommaSplitDelimiter);

        leaf.RankNeeded = int.Parse(split[0]);
        leaf.InternalFirstParameter = int.Parse(split[2]);
        leaf.InternalSecondParameter = int.Parse(split[3]);
        leaf.InternalThirdParameter = int.Parse(split[4]);
        leaf.InitializeFromExisting();
    }
}