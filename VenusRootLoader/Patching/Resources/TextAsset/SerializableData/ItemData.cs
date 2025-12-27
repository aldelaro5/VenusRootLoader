namespace VenusRootLoader.Patching.Resources.TextAsset.SerializableData;

internal sealed class ItemData : ITextAssetSerializable
{
    internal int BuyingPrice { get; set; }
    internal List<ItemUse> Effects { get; } = new();
    internal BattleControl.AttackArea Target { get; set; }

    public string GetTextAssetSerializedString() =>
        $"{BuyingPrice}@" +
        $"{string.Join(",", Effects.Select(e => e.GetTextAssetSerializedString()).ToArray())}@" +
        $"{Target}";
}