using VenusRootLoader.Patching.Resources.TextAsset;

namespace VenusRootLoader.TextAssetParsers.Entities;

internal sealed class MapEntityName : ITextAssetSerializable
{
    internal string Name { get; set; } = "";

    string ITextAssetSerializable.GetTextAssetSerializedString() => Name;

    void ITextAssetSerializable.FromTextAssetSerializedString(string text) => Name = text;
}