using VenusRootLoader.Extensions;

namespace VenusRootLoader.Patching.Resources.TextAsset.SerializableData;

internal sealed class ItemUse : ITextAssetSerializable
{
    internal MainManager.ItemUsage UseType { get; set; }
    internal int Value { get; set; }

    public string GetTextAssetSerializedString() => $"{UseType},{Value}";

    public void FromTextAssetSerializedString(string text)
    {
        string[] fields = text.Split(',');
        UseType = Enum.Parse<MainManager.ItemUsage>(fields[0]);
        Value = int.Parse(fields[1]);
    }
}