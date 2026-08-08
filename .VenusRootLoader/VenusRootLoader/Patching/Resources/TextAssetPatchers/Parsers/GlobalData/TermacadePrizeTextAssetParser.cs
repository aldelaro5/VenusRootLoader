using System.Globalization;
using System.Text;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers.GlobalData;

/// <inheritdoc/>
internal sealed class TermacadePrizeTextAssetParser : ITextAssetParser<TermacadePrizeLeaf>
{
    private enum PrizeAvailability
    {
        AlwaysAvailable,
        SingleTimePurchase
    }

    private readonly ILeavesRegistry<FlagLeaf> _flagsRegistry;

    public TermacadePrizeTextAssetParser(ILeavesRegistry<FlagLeaf> flagsRegistry)
    {
        _flagsRegistry = flagsRegistry;
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
            (int)(leaf.AlreadyBoughtFlag is null
                ? PrizeAvailability.AlwaysAvailable
                : PrizeAvailability.SingleTimePurchase));
        sb.Append(',');
        sb.Append(leaf.AlreadyBoughtFlag?.Resolve().GameId ?? 0);

        return sb.ToString();
    }

    public void FromTextAssetSerializedString(string subPath, string text, TermacadePrizeLeaf leaf)
    {
        string[] fields = text.Split(StringUtils.CommaSplitDelimiter);

        leaf.PrizeType = (TermacadePrizeLeaf.TermacadePrizeType)int.Parse(fields[0], CultureInfo.InvariantCulture);
        leaf.ItemOrMedalGameId = int.Parse(fields[1], CultureInfo.InvariantCulture);
        leaf.GameTokenCost = int.Parse(fields[2], CultureInfo.InvariantCulture);
        int availability = int.Parse(fields[3], CultureInfo.InvariantCulture);
        leaf.AlreadyBoughtFlag = availability != (int)PrizeAvailability.SingleTimePurchase
            ? (Branch<FlagLeaf>?)null
            : _flagsRegistry.GetByGameId(int.Parse(fields[4], CultureInfo.InvariantCulture));
    }
}