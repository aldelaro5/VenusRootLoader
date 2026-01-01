using VenusRootLoader.Patching.Resources.TextAsset;

namespace VenusRootLoader.Api.TextAssetData.ActionCommands;

public sealed class ActionCommandInstruction : ITextAssetSerializable
{
    public string Instructions { get; set; } = "";

    string ITextAssetSerializable.GetTextAssetSerializedString() => Instructions;

    void ITextAssetSerializable.FromTextAssetSerializedString(string text) => Instructions = text;
}