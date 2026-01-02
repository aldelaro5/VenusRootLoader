using System.Text;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Extensions;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.TextAssetData.Items;

internal sealed class ItemData : ITextAssetSerializable
{
    internal int BuyingPrice { get; set; }
    internal List<ItemLeaf.ItemUse> Effects { get; } = new();
    internal BattleControl.AttackArea Target { get; set; }

    string ITextAssetSerializable.GetTextAssetSerializedString()
    {
        StringBuilder sb = new();
        sb.Append(BuyingPrice);
        sb.Append('@');

        string[] serializedEffects = Effects
            .Select(e => $"{e.Effect},{e.Value}")
            .ToArray();
        sb.Append(string.Join(";", serializedEffects));

        sb.Append('@');
        sb.Append(Target);
        return sb.ToString();
    }

    void ITextAssetSerializable.FromTextAssetSerializedString(string text)
    {
        string[] fields = text.Split(StringUtils.AtSymbolSplitDelimiter);
        if (!string.IsNullOrWhiteSpace(fields[0]))
            BuyingPrice = int.Parse(fields[0]);
        if (!string.IsNullOrWhiteSpace(fields[1]))
        {
            string[] effects = fields[1].Split(StringUtils.SemiColonSplitDelimiter);
            Effects.Clear();
            foreach (string effect in effects)
            {
                ItemLeaf.ItemUse itemUse = new();
                string[] effectFields = effect.Split(StringUtils.CommaSplitDelimiter);
                itemUse.Effect = Enum.Parse<MainManager.ItemUsage>(effectFields[0]);
                itemUse.Value = int.Parse(effectFields[1]);
                Effects.Add(itemUse);
            }
        }

        if (!string.IsNullOrWhiteSpace(fields[2]))
            Target = Enum.Parse<BattleControl.AttackArea>(fields[2]);
    }
}