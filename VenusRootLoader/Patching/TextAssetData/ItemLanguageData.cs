namespace VenusRootLoader.Patching.TextAssetData;

internal sealed class ItemLanguageData : ITextAssetSerializable
{
    internal required string Name { get; set; } = "<NO NAME>";
    internal required string UnusedDescription { get; set; } = "";
    internal required string Description { get; set; } = "<NO DESCRIPTION>";
    internal required string Prepender { get; set; } = "";

    public string GetTextAssetSerializedString() => $"{Name}@{UnusedDescription}@{Description}@{Prepender}";
}