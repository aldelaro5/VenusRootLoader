using VenusRootLoader.Patching.Resources.TextAsset;

namespace VenusRootLoader.Api.TextAssetData.DialoguesLines;

public sealed class FishingDialogueLine : ITextAssetSerializable
{
    public string Text { get; set; } = "";

    string ITextAssetSerializable.GetTextAssetSerializedString() => Text;

    void ITextAssetSerializable.FromTextAssetSerializedString(string text) => Text = text;
}