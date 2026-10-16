using System.Globalization;
using System.Text;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Extensions;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers.GlobalData;

/// <inheritdoc/>
internal sealed class ItemTextAssetParser : ITextAssetParser<ItemLeaf>
{
    private readonly ILeavesRegistry<ItemUsageLeaf> _itemUsagesRegistry;

    public ItemTextAssetParser(ILeavesRegistry<ItemUsageLeaf> itemUsagesRegistry)
    {
        _itemUsagesRegistry = itemUsagesRegistry;
    }

    public string GetTextAssetSerializedString(string subPath, ItemLeaf leaf)
    {
        StringBuilder sb = new();
        sb.Append(leaf.BuyingPrice);
        sb.Append('@');

        string[] serializedEffects = leaf.Effects
            .Select(e => $"{e.Effect.Resolve().EffectiveId},{e.Value}")
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
            leaf.BuyingPrice = int.Parse(fields[0], CultureInfo.InvariantCulture);
        if (!string.IsNullOrWhiteSpace(fields[1]))
        {
            string[] effects = fields[1].Split(StringUtils.SemiColonSplitDelimiter);
            leaf.Effects.Clear();
            foreach (string effect in effects)
            {
                string[] effectFields = effect.Split(StringUtils.CommaSplitDelimiter);
                int usageGameId = (int)Enum.Parse<MainManager.ItemUsage>(effectFields[0]);
                ItemUsageLeaf usage = _itemUsagesRegistry.GetByGameId(usageGameId);
                int value = int.Parse(effectFields[1], CultureInfo.InvariantCulture);
                ItemLeaf.ItemUse itemUse = new()
                {
                    Effect = usage,
                    Value = value
                };
                leaf.Effects.Add(itemUse);
            }
        }

        if (!string.IsNullOrWhiteSpace(fields[2]))
            leaf.Target = Enum.Parse<BattleControl.AttackArea>(fields[2]);
    }
}