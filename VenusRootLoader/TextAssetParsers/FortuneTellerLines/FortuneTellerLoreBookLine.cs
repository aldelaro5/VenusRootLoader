using VenusRootLoader.Patching.Resources.TextAsset;

namespace VenusRootLoader.TextAssetParsers.FortuneTellerLines;

internal class FortuneTellerLoreBookLine : ITextAssetSerializable
{
    internal string HintLine { get; set; } = "";

    string ITextAssetSerializable.GetTextAssetSerializedString() => HintLine;

    void ITextAssetSerializable.FromTextAssetSerializedString(string text) => HintLine = text;
}