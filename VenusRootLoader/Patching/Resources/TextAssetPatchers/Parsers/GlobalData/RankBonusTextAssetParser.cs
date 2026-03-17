using System.Text;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers.GlobalData;

internal sealed class RankBonusTextAssetParser : ITextAssetParser<RankBonusLeaf>
{
    public string GetTextAssetSerializedString(string subPath, RankBonusLeaf value)
    {
        StringBuilder sb = new();

        sb.Append(value.RankNeeded);
        sb.Append(',');
        sb.Append((int)value.BonusType);
        sb.Append(',');
        sb.Append(value.FirstParameter);
        sb.Append(',');
        sb.Append(value.SecondParameter);
        sb.Append(',');
        sb.Append(value.ThirdParameter);

        return sb.ToString();
    }

    public void FromTextAssetSerializedString(string subPath, string text, RankBonusLeaf value)
    {
        string[] split = text.Split(StringUtils.CommaSplitDelimiter);

        value.RankNeeded = int.Parse(split[0]);
        value.BonusType = (RankBonusLeaf.RankBonusType)int.Parse(split[1]);
        value.FirstParameter = int.Parse(split[2]);
        value.SecondParameter = int.Parse(split[3]);
        value.ThirdParameter = int.Parse(split[4]);
    }
}