using VenusRootLoader.Patching.Resources.TextAsset;

namespace VenusRootLoader.TextAssetData.DialoguesLines;

internal sealed class FishingDialogueLine : ITextAssetSerializable
{
    internal string Text { get; set; } = "";

    string ITextAssetSerializable.GetTextAssetSerializedString() => Text;

    void ITextAssetSerializable.FromTextAssetSerializedString(string text) => Text = text;
}