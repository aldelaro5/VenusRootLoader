using VenusRootLoader.Patching.Resources.TextAsset;

namespace VenusRootLoader.Api.TextAssetData.Musics;

public sealed class MusicLanguageData : ITextAssetSerializable
{
    public string Title { get; set; } = "";

    string ITextAssetSerializable.GetTextAssetSerializedString() => Title;

    void ITextAssetSerializable.FromTextAssetSerializedString(string text) => Title = text;
}