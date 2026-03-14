using System.Text;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers.GlobalData;

internal sealed class RankBonusTextAssetParser : ITextAssetParser<RankBonusLeaf>
{
    public string GetTextAssetSerializedString(string subPath, RankBonusLeaf leaf)
    {
        StringBuilder sb = new();

        sb.Append(leaf.RankNeeded);
        sb.Append(',');
        sb.Append((int)leaf.BonusType);
        sb.Append(',');
        sb.Append(leaf.FirstParameter);
        sb.Append(',');
        sb.Append(leaf.SecondParameter);
        sb.Append(',');
        sb.Append(leaf.ThirdParameter);

        return sb.ToString();
    }

    public void FromTextAssetSerializedString(string subPath, string text, RankBonusLeaf leaf)
    {
        string[] split = text.Split(StringUtils.CommaSplitDelimiter);

        leaf.RankNeeded = int.Parse(split[0]);
        leaf.BonusType = (RankBonusLeaf.RankBonusType)int.Parse(split[1]);
        leaf.FirstParameter = int.Parse(split[2]);
        leaf.SecondParameter = int.Parse(split[3]);
        leaf.ThirdParameter = int.Parse(split[4]);
    }
}