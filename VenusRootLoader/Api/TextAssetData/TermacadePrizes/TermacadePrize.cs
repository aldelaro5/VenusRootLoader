using System.Text;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Api.TextAssetData.TermacadePrizes;

public sealed class TermacadePrize : ITextAssetSerializable
{
    public int Type { get; set; }
    public int ItemOrMedalId { get; set; }
    public int GameTokenCost { get; set; }
    public int Availability { get; set; }
    public int BoundPurchasedFlagId { get; set; }

    string ITextAssetSerializable.GetTextAssetSerializedString()
    {
        StringBuilder sb = new();

        sb.Append(Type);
        sb.Append(',');
        sb.Append(ItemOrMedalId);
        sb.Append(',');
        sb.Append(GameTokenCost);
        sb.Append(',');
        sb.Append(Availability);
        sb.Append(',');
        sb.Append(BoundPurchasedFlagId);

        return sb.ToString();
    }

    void ITextAssetSerializable.FromTextAssetSerializedString(string text)
    {
        string[] fields = text.Split(StringUtils.CommaSplitDelimiter);

        Type = int.Parse(fields[0]);
        ItemOrMedalId = int.Parse(fields[1]);
        GameTokenCost = int.Parse(fields[2]);
        Availability = int.Parse(fields[3]);
        BoundPurchasedFlagId = int.Parse(fields[4]);
    }
}