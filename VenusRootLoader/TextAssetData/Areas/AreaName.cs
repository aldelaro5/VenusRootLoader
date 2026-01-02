using VenusRootLoader.Patching.Resources.TextAsset;

namespace VenusRootLoader.TextAssetData.Areas;

internal sealed class AreaName : ITextAssetSerializable
{
    internal string Name { get; set; } = "";

    string ITextAssetSerializable.GetTextAssetSerializedString() => Name;

    void ITextAssetSerializable.FromTextAssetSerializedString(string text) => Name = text;
}