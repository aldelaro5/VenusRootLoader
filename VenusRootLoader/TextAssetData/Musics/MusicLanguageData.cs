using VenusRootLoader.Patching.Resources.TextAsset;

namespace VenusRootLoader.TextAssetData.Musics;

internal sealed class MusicLanguageData : ITextAssetSerializable
{
    internal string Title { get; set; } = "";

    string ITextAssetSerializable.GetTextAssetSerializedString() => Title;

    void ITextAssetSerializable.FromTextAssetSerializedString(string text) => Title = text;
}