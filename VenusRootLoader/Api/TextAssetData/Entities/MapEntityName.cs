using VenusRootLoader.Patching.Resources.TextAsset;

namespace VenusRootLoader.Api.TextAssetData.Entities;

public sealed class MapEntityName : ITextAssetSerializable
{
    public string Name { get; set; } = "";

    string ITextAssetSerializable.GetTextAssetSerializedString() => Name;

    void ITextAssetSerializable.FromTextAssetSerializedString(string text) => Name = text;
}