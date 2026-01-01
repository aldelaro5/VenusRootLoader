using VenusRootLoader.Patching.Resources.TextAsset;

namespace VenusRootLoader.Api.TextAssetData.Miscellaneous;

public sealed class LanguageHelp : ITextAssetSerializable
{
    public string HelpText { get; set; } = "";

    string ITextAssetSerializable.GetTextAssetSerializedString() => HelpText;

    void ITextAssetSerializable.FromTextAssetSerializedString(string text) => HelpText = text;
}