using System.Text;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Extensions;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.TextAssetData.Items;

internal sealed class ItemDataSerializer : ITextAssetSerializable<ItemLeaf, int>
{
    public string GetTextAssetSerializedString(ItemLeaf item)
    {
        StringBuilder sb = new();
        sb.Append(item.BuyingPrice);
        sb.Append('@');

        string[] serializedEffects = item.Effects
            .Select(e => $"{e.Effect},{e.Value}")
            .ToArray();
        sb.Append(string.Join(";", serializedEffects));

        sb.Append('@');
        sb.Append(item.Target);
        return sb.ToString();
    }

    public void FromTextAssetSerializedString(string text, ItemLeaf item)
    {
        string[] fields = text.Split(StringUtils.AtSymbolSplitDelimiter);
        if (!string.IsNullOrWhiteSpace(fields[0]))
            item.BuyingPrice = int.Parse(fields[0]);
        if (!string.IsNullOrWhiteSpace(fields[1]))
        {
            string[] effects = fields[1].Split(StringUtils.SemiColonSplitDelimiter);
            item.Effects.Clear();
            foreach (string effect in effects)
            {
                ItemLeaf.ItemUse itemUse = new();
                string[] effectFields = effect.Split(StringUtils.CommaSplitDelimiter);
                itemUse.Effect = Enum.Parse<MainManager.ItemUsage>(effectFields[0]);
                itemUse.Value = int.Parse(effectFields[1]);
                item.Effects.Add(itemUse);
            }
        }

        if (!string.IsNullOrWhiteSpace(fields[2]))
            item.Target = Enum.Parse<BattleControl.AttackArea>(fields[2]);
    }
}