using System.Globalization;
using System.Text;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.TextAsset.Parsers;

internal sealed class TermacadePrizeTextAssetParser : ITextAssetParser<TermacadePrizeLeaf>
{
    private enum PrizeAvailability
    {
        AlwaysAvailable,
        SingleTimePurchase
    }
    
    public string GetTextAssetSerializedString(string subPath, TermacadePrizeLeaf leaf)
    {
        StringBuilder sb = new();

        sb.Append((int)leaf.PrizeType);
        sb.Append(',');
        sb.Append(leaf.ItemOrMedalGameId);
        sb.Append(',');
        sb.Append(leaf.GameTokenCost);
        sb.Append(',');
        sb.Append(
            (int)(leaf.AlreadyBoughtFlagGameId is null
                ? PrizeAvailability.AlwaysAvailable
                : PrizeAvailability.SingleTimePurchase));
        sb.Append(',');
        sb.Append(leaf.AlreadyBoughtFlagGameId ?? 0);

        return sb.ToString();
    }

    public void FromTextAssetSerializedString(string subPath, string text, TermacadePrizeLeaf leaf)
    {
        string[] fields = text.Split(StringUtils.CommaSplitDelimiter);

        leaf.PrizeType = (TermacadePrizeLeaf.TermacadePrizeType)int.Parse(fields[0], CultureInfo.InvariantCulture);
        leaf.ItemOrMedalGameId = int.Parse(fields[1], CultureInfo.InvariantCulture);
        leaf.GameTokenCost = int.Parse(fields[2], CultureInfo.InvariantCulture);
        int availability = int.Parse(fields[3], CultureInfo.InvariantCulture);
        leaf.AlreadyBoughtFlagGameId = availability != (int)PrizeAvailability.SingleTimePurchase
            ? null
            : int.Parse(fields[4], CultureInfo.InvariantCulture);
    }
}