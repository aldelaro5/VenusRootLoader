namespace VenusRootLoader.Patching.Resources.TextAsset.SerializableData;

internal sealed class ItemUse : ITextAssetSerializable
{
    internal required MainManager.ItemUsage UseType { get; set; }
    internal required int Value { get; set; }

    public string GetTextAssetSerializedString() => $"{UseType},{Value}";
}