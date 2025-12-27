namespace VenusRootLoader.Patching.TextAssetData;

internal sealed class ItemLanguageData : ITextAssetSerializable
{
    internal string Name { get; set; } = "<NO NAME>";
    internal string UnusedDescription { get; set; } = "";
    internal string Description { get; set; } = "<NO DESCRIPTION>";
    internal string Prepender { get; set; } = "";

    public string GetTextAssetSerializedString() => $"{Name}@{UnusedDescription}@{Description}@{Prepender}";
}