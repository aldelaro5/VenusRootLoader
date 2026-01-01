using System.Text;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Api.TextAssetData.RankBonuses;

public sealed class RankBonusData : ITextAssetSerializable
{
    public int Rank { get; set; }
    public int BonusType { get; set; }
    public int FirstParameterValue { get; set; }
    public int SecondParameterValue { get; set; }
    public int ThirdParameterValue { get; set; }

    string ITextAssetSerializable.GetTextAssetSerializedString()
    {
        StringBuilder sb = new();

        sb.Append(Rank);
        sb.Append(',');
        sb.Append(BonusType);
        sb.Append(',');
        sb.Append(FirstParameterValue);
        sb.Append(',');
        sb.Append(SecondParameterValue);
        sb.Append(',');
        sb.Append(ThirdParameterValue);

        return sb.ToString();
    }

    void ITextAssetSerializable.FromTextAssetSerializedString(string text)
    {
        string[] split = text.Split(StringUtils.CommaSplitDelimiter);

        Rank = int.Parse(split[0]);
        BonusType = int.Parse(split[1]);
        FirstParameterValue = int.Parse(split[2]);
        SecondParameterValue = int.Parse(split[3]);
        ThirdParameterValue = int.Parse(split[4]);
    }
}