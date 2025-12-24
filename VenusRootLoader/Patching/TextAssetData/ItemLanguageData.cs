namespace VenusRootLoader.Patching.TextAssetData;

internal sealed class ItemLanguageData : ITextAssetSerializable
{
    internal required string Name { get; set; } = "";
    internal required string UnusedDescription { get; set; } = "";
    internal required string Description { get; set; } = "";
    internal required string Prepender { get; set; } = "";

    public string GetTextAssetSerializedString() => $"{Name}@{UnusedDescription}@{Description}@{Prepender}";
}