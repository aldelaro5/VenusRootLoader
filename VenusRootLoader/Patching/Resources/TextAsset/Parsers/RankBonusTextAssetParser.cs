using System.Text;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.TextAsset.Parsers;

internal sealed class RankBonusTextAssetParser : ITextAssetParser<RankBonusLeaf>
{
    public string GetTextAssetSerializedString(string subPath, RankBonusLeaf leaf)
    {
        StringBuilder sb = new();

        sb.Append(leaf.Rank);
        sb.Append(',');
        sb.Append(leaf.BonusType);
        sb.Append(',');
        sb.Append(leaf.FirstParameterValue);
        sb.Append(',');
        sb.Append(leaf.SecondParameterValue);
        sb.Append(',');
        sb.Append(leaf.ThirdParameterValue);

        return sb.ToString();
    }

    public void FromTextAssetSerializedString(string subPath, string text, RankBonusLeaf leaf)
    {
        string[] split = text.Split(StringUtils.CommaSplitDelimiter);

        leaf.Rank = int.Parse(split[0]);
        leaf.BonusType = int.Parse(split[1]);
        leaf.FirstParameterValue = int.Parse(split[2]);
        leaf.SecondParameterValue = int.Parse(split[3]);
        leaf.ThirdParameterValue = int.Parse(split[4]);
    }
}