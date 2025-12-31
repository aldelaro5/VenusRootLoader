using System.Text;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Api.TextAssetData.Medals;

public sealed class MedalData : ITextAssetSerializable
{
    public int MpCost { get; set; }
    public bool IsPartyEquip { get; set; }
    public List<MedalEffect> Effects { get; } = new();
    public int BuyingPriceRegularBerries { get; set; }
    public int BuyingPriceCrystalBerries { get; set; }
    internal int Items1SpriteIndex { get; set; } = -1;

    string ITextAssetSerializable.GetTextAssetSerializedString()
    {
        StringBuilder sb = new();
        sb.Append(MpCost);
        sb.Append('@');
        sb.Append(IsPartyEquip);
        sb.Append('@');

        string[] serializedEffects = Effects
            .Cast<ITextAssetSerializable>()
            .Select(e => e.GetTextAssetSerializedString())
            .ToArray();
        sb.Append(string.Join(";", serializedEffects));

        sb.Append('@');
        sb.Append(BuyingPriceRegularBerries);
        sb.Append('@');
        sb.Append(BuyingPriceCrystalBerries);
        sb.Append('@');
        sb.Append(Items1SpriteIndex);
        return sb.ToString();
    }

    void ITextAssetSerializable.FromTextAssetSerializedString(string text)
    {
        string[] fields = text.Split(StringUtils.AtSymbolSplitDelimiter);
        MpCost = int.Parse(fields[0]);
        IsPartyEquip = bool.Parse(fields[1]);

        string[] effects = fields[2].Split(StringUtils.SemiColonSplitDelimiter);
        Effects.Clear();
        foreach (string effect in effects)
        {
            MedalEffect medalEffect = new();
            ((ITextAssetSerializable)medalEffect).FromTextAssetSerializedString(effect);
            Effects.Add(medalEffect);
        }

        BuyingPriceRegularBerries = int.Parse(fields[3]);
        BuyingPriceCrystalBerries = int.Parse(fields[4]);
        Items1SpriteIndex = int.Parse(fields[5]);
    }
}