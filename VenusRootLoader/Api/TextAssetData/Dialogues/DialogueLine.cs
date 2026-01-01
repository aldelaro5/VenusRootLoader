using VenusRootLoader.Patching.Resources.TextAsset;

namespace VenusRootLoader.Api.TextAssetData.Dialogues;

public sealed class DialogueLine : ITextAssetSerializable
{
    public string Text { get; set; }

    string ITextAssetSerializable.GetTextAssetSerializedString() => Text;

    void ITextAssetSerializable.FromTextAssetSerializedString(string text) => Text = text;
}