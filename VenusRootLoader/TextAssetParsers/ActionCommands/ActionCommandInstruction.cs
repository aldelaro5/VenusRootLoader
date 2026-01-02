using VenusRootLoader.Patching.Resources.TextAsset;

namespace VenusRootLoader.TextAssetParsers.ActionCommands;

internal sealed class ActionCommandInstruction : ITextAssetSerializable
{
    internal string Instructions { get; set; } = "";

    string ITextAssetSerializable.GetTextAssetSerializedString() => Instructions;

    void ITextAssetSerializable.FromTextAssetSerializedString(string text) => Instructions = text;
}