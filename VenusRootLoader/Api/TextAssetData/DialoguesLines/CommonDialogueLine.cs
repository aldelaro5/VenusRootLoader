using VenusRootLoader.Patching.Resources.TextAsset;

namespace VenusRootLoader.Api.TextAssetData.DialoguesLines;

public sealed class CommonDialogueLine : ITextAssetSerializable
{
    public string Text { get; set; } = "";

    string ITextAssetSerializable.GetTextAssetSerializedString() => Text;

    void ITextAssetSerializable.FromTextAssetSerializedString(string text) => Text = text;
}