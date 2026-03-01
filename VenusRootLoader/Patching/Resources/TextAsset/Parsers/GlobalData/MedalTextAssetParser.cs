using System.Globalization;
using System.Text;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Extensions;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.TextAsset.Parsers.GlobalData;

internal sealed class MedalTextAssetParser : ITextAssetParser<MedalLeaf>
{
    public string GetTextAssetSerializedString(string subPath, MedalLeaf leaf)
    {
        StringBuilder sb = new();
        sb.Append(leaf.MpCost);
        sb.Append('@');
        sb.Append(leaf.IsPartyEquip);
        sb.Append('@');

        IEnumerable<string> serializedEffects = leaf.Effects
            .Select(e => $"{e.Effect},{e.Value}");
        sb.Append(string.Join(";", serializedEffects));

        sb.Append('@');
        sb.Append(leaf.BuyingPriceRegularBerries);
        sb.Append('@');
        sb.Append(leaf.BuyingPriceCrystalBerries);
        sb.Append('@');
        sb.Append(leaf.Items1SpriteIndex);
        return sb.ToString();
    }

    public void FromTextAssetSerializedString(string subPath, string text, MedalLeaf leaf)
    {
        string[] fields = text.Split(StringUtils.AtSymbolSplitDelimiter);
        leaf.MpCost = int.Parse(fields[0], CultureInfo.InvariantCulture);
        leaf.IsPartyEquip = bool.Parse(fields[1]);

        string[] effects = fields[2].Split(StringUtils.SemiColonSplitDelimiter);
        leaf.Effects.Clear();
        foreach (string effect in effects)
        {
            MedalLeaf.MedalEffect medalEffect = new();
            string[] effectFields = effect.Split(StringUtils.CommaSplitDelimiter);
            medalEffect.Effect = Enum.Parse<MainManager.BadgeEffects>(effectFields[0]);
            medalEffect.Value = int.Parse(effectFields[1], CultureInfo.InvariantCulture);
            leaf.Effects.Add(medalEffect);
        }

        leaf.BuyingPriceRegularBerries = int.Parse(fields[3], CultureInfo.InvariantCulture);
        leaf.BuyingPriceCrystalBerries = int.Parse(fields[4], CultureInfo.InvariantCulture);
        leaf.Items1SpriteIndex = int.Parse(fields[5], CultureInfo.InvariantCulture);
    }
}