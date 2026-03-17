using System.Globalization;
using System.Text;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Extensions;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers.GlobalData;

internal sealed class ItemTextAssetParser : ITextAssetParser<ItemLeaf>
{
    public string GetTextAssetSerializedString(string subPath, ItemLeaf value)
    {
        StringBuilder sb = new();
        sb.Append(value.BuyingPrice);
        sb.Append('@');

        string[] serializedEffects = value.Effects
            .Select(e => $"{e.Effect},{e.Value}")
            .ToArray();
        sb.Append(string.Join(";", serializedEffects));

        sb.Append('@');
        sb.Append(value.Target);
        return sb.ToString();
    }

    public void FromTextAssetSerializedString(string subPath, string text, ItemLeaf value)
    {
        string[] fields = text.Split(StringUtils.AtSymbolSplitDelimiter);
        if (!string.IsNullOrWhiteSpace(fields[0]))
            value.BuyingPrice = int.Parse(fields[0], CultureInfo.InvariantCulture);
        if (!string.IsNullOrWhiteSpace(fields[1]))
        {
            string[] effects = fields[1].Split(StringUtils.SemiColonSplitDelimiter);
            value.Effects.Clear();
            foreach (string effect in effects)
            {
                ItemLeaf.ItemUse itemUse = new();
                string[] effectFields = effect.Split(StringUtils.CommaSplitDelimiter);
                itemUse.Effect = Enum.Parse<MainManager.ItemUsage>(effectFields[0]);
                itemUse.Value = int.Parse(effectFields[1], CultureInfo.InvariantCulture);
                value.Effects.Add(itemUse);
            }
        }

        if (!string.IsNullOrWhiteSpace(fields[2]))
            value.Target = Enum.Parse<BattleControl.AttackArea>(fields[2]);
    }
}