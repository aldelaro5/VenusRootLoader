using System.Globalization;
using System.Text;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers.GlobalData;

internal sealed class TermacadePrizeTextAssetParser : ITextAssetParser<TermacadePrizeLeaf>
{
    private enum PrizeAvailability
    {
        AlwaysAvailable,
        SingleTimePurchase
    }

    public string GetTextAssetSerializedString(string subPath, TermacadePrizeLeaf value)
    {
        StringBuilder sb = new();

        sb.Append((int)value.PrizeType);
        sb.Append(',');
        sb.Append(value.ItemOrMedalGameId);
        sb.Append(',');
        sb.Append(value.GameTokenCost);
        sb.Append(',');
        sb.Append(
            (int)(value.AlreadyBoughtFlagGameId is null
                ? PrizeAvailability.AlwaysAvailable
                : PrizeAvailability.SingleTimePurchase));
        sb.Append(',');
        sb.Append(value.AlreadyBoughtFlagGameId ?? 0);

        return sb.ToString();
    }

    public void FromTextAssetSerializedString(string subPath, string text, TermacadePrizeLeaf value)
    {
        string[] fields = text.Split(StringUtils.CommaSplitDelimiter);

        value.PrizeType = (TermacadePrizeLeaf.TermacadePrizeType)int.Parse(fields[0], CultureInfo.InvariantCulture);
        value.ItemOrMedalGameId = int.Parse(fields[1], CultureInfo.InvariantCulture);
        value.GameTokenCost = int.Parse(fields[2], CultureInfo.InvariantCulture);
        int availability = int.Parse(fields[3], CultureInfo.InvariantCulture);
        value.AlreadyBoughtFlagGameId = availability != (int)PrizeAvailability.SingleTimePurchase
            ? null
            : int.Parse(fields[4], CultureInfo.InvariantCulture);
    }
}