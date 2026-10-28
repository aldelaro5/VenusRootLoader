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

        sb.Append(leaf.RankNeeded);
        sb.Append(',');
        sb.Append((int)leaf.InternalBonusType);
        sb.Append(',');
        sb.Append(leaf.InternalFirstParameter);
        sb.Append(',');
        sb.Append(leaf.InternalSecondParameter);
        sb.Append(',');
        sb.Append(leaf.InternalThirdParameter);

        return sb.ToString();
    }

    public void FromTextAssetSerializedString(string subPath, string text, RankBonusLeaf leaf)
    {
        string[] split = text.Split(StringUtils.CommaSplitDelimiter);

        leaf.RankNeeded = int.Parse(split[0]);
        leaf.InternalBonusType = (RankBonusLeaf.RankBonusType)int.Parse(split[1]);
        leaf.InternalFirstParameter = int.Parse(split[2]);
        leaf.InternalSecondParameter = int.Parse(split[3]);
        leaf.InternalThirdParameter = int.Parse(split[4]);
    }
}