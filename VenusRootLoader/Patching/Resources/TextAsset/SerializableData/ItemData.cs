using VenusRootLoader.Extensions;

namespace VenusRootLoader.Patching.Resources.TextAsset.SerializableData;

internal sealed class ItemData : ITextAssetSerializable
{
    internal int BuyingPrice { get; set; }
    internal List<ItemUse> Effects { get; } = new();
    internal BattleControl.AttackArea Target { get; set; }

    public string GetTextAssetSerializedString() =>
        $"{BuyingPrice}@" +
        $"{string.Join(";", Effects.Select(e => e.GetTextAssetSerializedString()).ToArray())}@" +
        $"{Target}";

    public void FromTextAssetSerializedString(string text)
    {
        string[] fields = text.Split('@');
        if (!string.IsNullOrWhiteSpace(fields[0]))
            BuyingPrice = int.Parse(fields[0]);
        if (!string.IsNullOrWhiteSpace(fields[1]))
        {
            string[] effects = fields[1].Split(';');
            Effects.Clear();
            foreach (string effect in effects)
            {
                ItemUse itemUse = new();
                itemUse.FromTextAssetSerializedString(effect);
                Effects.Add(itemUse);
            }
        }

        if (!string.IsNullOrWhiteSpace(fields[2]))
            Target = Enum.Parse<BattleControl.AttackArea>(fields[2]);
    }
}