using VenusRootLoader.Patching.Resources.TextAsset;

namespace VenusRootLoader.TextAssetParsers.DialoguesLines;

internal sealed class MapDialogueLine : ITextAssetSerializable
{
    internal string Text { get; set; } = "";

    string ITextAssetSerializable.GetTextAssetSerializedString() => Text;

    void ITextAssetSerializable.FromTextAssetSerializedString(string text) => Text = text;
}