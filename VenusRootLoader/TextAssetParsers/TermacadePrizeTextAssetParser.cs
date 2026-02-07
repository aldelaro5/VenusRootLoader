using System.Text;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.TextAssetParsers;

internal sealed class TermacadePrizeTextAssetParser : ITextAssetParser<TermacadePrizeLeaf>
{
    public string GetTextAssetSerializedString(string subPath, TermacadePrizeLeaf leaf)
    {
        StringBuilder sb = new();

        sb.Append(leaf.Type);
        sb.Append(',');
        sb.Append(leaf.ItemOrMedalId);
        sb.Append(',');
        sb.Append(leaf.GameTokenCost);
        sb.Append(',');
        sb.Append(leaf.Availability);
        sb.Append(',');
        sb.Append(leaf.BoundPurchasedFlagId);

        return sb.ToString();
    }

    public void FromTextAssetSerializedString(string subPath, string text, TermacadePrizeLeaf leaf)
    {
        string[] fields = text.Split(StringUtils.CommaSplitDelimiter);

        leaf.Type = int.Parse(fields[0]);
        leaf.ItemOrMedalId = int.Parse(fields[1]);
        leaf.GameTokenCost = int.Parse(fields[2]);
        leaf.Availability = int.Parse(fields[3]);
        leaf.BoundPurchasedFlagId = int.Parse(fields[4]);
    }
}