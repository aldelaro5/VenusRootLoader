using VenusRootLoader.Patching.Resources.TextAsset;

namespace VenusRootLoader.TextAssetParsers.Miscellaneous;

internal sealed class LanguageHelp : ITextAssetSerializable
{
    internal string HelpText { get; set; } = "";

    string ITextAssetSerializable.GetTextAssetSerializedString() => HelpText;

    void ITextAssetSerializable.FromTextAssetSerializedString(string text) => HelpText = text;
}