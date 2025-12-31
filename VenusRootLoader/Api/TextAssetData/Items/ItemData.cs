using System.Text;
using VenusRootLoader.Extensions;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Api.TextAssetData.Items;

public sealed class ItemData : ITextAssetSerializable
{
    public int BuyingPrice { get; set; }
    public List<ItemUse> Effects { get; } = new();
    public BattleControl.AttackArea Target { get; set; }

    string ITextAssetSerializable.GetTextAssetSerializedString()
    {
        StringBuilder sb = new();
        sb.Append(BuyingPrice);
        sb.Append('@');

        string[] serializedEffects = Effects
            .Cast<ITextAssetSerializable>()
            .Select(e => e.GetTextAssetSerializedString())
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
                ItemUse itemUse = new();
                ((ITextAssetSerializable)itemUse).FromTextAssetSerializedString(effect);
                Effects.Add(itemUse);
            }
        }

        if (!string.IsNullOrWhiteSpace(fields[2]))
            Target = Enum.Parse<BattleControl.AttackArea>(fields[2]);
    }
}