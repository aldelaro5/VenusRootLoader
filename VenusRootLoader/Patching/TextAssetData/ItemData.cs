namespace VenusRootLoader.Patching.TextAssetData;

internal sealed class ItemData : ITextAssetSerializable
{
    internal required int BuyingPrice { get; set; }
    internal required List<ItemUse> Effects { get; set; }
    internal required BattleControl.AttackArea Target { get; set; }

    public string GetTextAssetSerializedString() =>
        $"{BuyingPrice}@" +
        $"{string.Join(",", Effects.Select(e => e.GetTextAssetSerializedString()).ToArray())}@" +
        $"{Target}";
}