using System.Text;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Extensions;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.TextAssetParsers;

internal sealed class ItemTextAssetParser : ITextAssetSerializable<ItemLeaf>
{
    public string GetTextAssetSerializedString(string subPath, ItemLeaf leaf)
    {
        StringBuilder sb = new();
        sb.Append(leaf.BuyingPrice);
        sb.Append('@');

        string[] serializedEffects = leaf.Effects
            .Select(e => $"{e.Effect},{e.Value}")
            .ToArray();
        sb.Append(string.Join(";", serializedEffects));

        sb.Append('@');
        sb.Append(leaf.Target);
        return sb.ToString();
    }

    public void FromTextAssetSerializedString(string subPath, string text, ItemLeaf leaf)
    {
        string[] fields = text.Split(StringUtils.AtSymbolSplitDelimiter);
        if (!string.IsNullOrWhiteSpace(fields[0]))
            leaf.BuyingPrice = int.Parse(fields[0]);
        if (!string.IsNullOrWhiteSpace(fields[1]))
        {
            string[] effects = fields[1].Split(StringUtils.SemiColonSplitDelimiter);
            leaf.Effects.Clear();
            foreach (string effect in effects)
            {
                ItemLeaf.ItemUse itemUse = new();
                string[] effectFields = effect.Split(StringUtils.CommaSplitDelimiter);
                itemUse.Effect = Enum.Parse<MainManager.ItemUsage>(effectFields[0]);
                itemUse.Value = int.Parse(effectFields[1]);
                leaf.Effects.Add(itemUse);
            }
        }

        if (!string.IsNullOrWhiteSpace(fields[2]))
            leaf.Target = Enum.Parse<BattleControl.AttackArea>(fields[2]);
    }
}