namespace VenusRootLoader.Patching.Resources.TextAsset.SerializableData;

internal sealed class ItemLanguageData : ITextAssetSerializable
{
    internal string Name { get; set; } = "<NO NAME>";
    internal string UnusedDescription { get; set; } = "";
    internal string Description { get; set; } = "<NO DESCRIPTION>";
    internal string Prepender { get; set; } = "";

    public string GetTextAssetSerializedString() => $"{Name}@{UnusedDescription}@{Description}@{Prepender}";
}