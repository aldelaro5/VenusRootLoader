using System.Globalization;
using System.Text;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Extensions;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers.GlobalData;

internal sealed class MedalTextAssetParser : ITextAssetParser<MedalLeaf>
{
    public string GetTextAssetSerializedString(string subPath, MedalLeaf value)
    {
        StringBuilder sb = new();
        sb.Append(value.MpCost);
        sb.Append('@');
        sb.Append(value.IsPartyEquip);
        sb.Append('@');

        IEnumerable<string> serializedEffects = value.Effects
            .Select(e => $"{e.Effect},{e.Value}");
        sb.Append(string.Join(";", serializedEffects));

        sb.Append('@');
        sb.Append(value.BuyingPriceRegularBerries);
        sb.Append('@');
        sb.Append(value.BuyingPriceCrystalBerries);
        sb.Append('@');
        sb.Append(value.Items1SpriteIndex);
        return sb.ToString();
    }

    public void FromTextAssetSerializedString(string subPath, string text, MedalLeaf value)
    {
        string[] fields = text.Split(StringUtils.AtSymbolSplitDelimiter);
        value.MpCost = int.Parse(fields[0], CultureInfo.InvariantCulture);
        value.IsPartyEquip = bool.Parse(fields[1]);

        string[] effects = fields[2].Split(StringUtils.SemiColonSplitDelimiter);
        value.Effects.Clear();
        foreach (string effect in effects)
        {
            MedalLeaf.MedalEffect medalEffect = new();
            string[] effectFields = effect.Split(StringUtils.CommaSplitDelimiter);
            medalEffect.Effect = Enum.Parse<MainManager.BadgeEffects>(effectFields[0]);
            medalEffect.Value = int.Parse(effectFields[1], CultureInfo.InvariantCulture);
            value.Effects.Add(medalEffect);
        }

        value.BuyingPriceRegularBerries = int.Parse(fields[3], CultureInfo.InvariantCulture);
        value.BuyingPriceCrystalBerries = int.Parse(fields[4], CultureInfo.InvariantCulture);
        value.Items1SpriteIndex = int.Parse(fields[5], CultureInfo.InvariantCulture);
    }
}