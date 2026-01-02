using VenusRootLoader.Patching.Resources.TextAsset;

namespace VenusRootLoader.TextAssetData.FortuneTellerLines;

internal class FortuneTellerCrystalBerryLine : ITextAssetSerializable
{
    internal string HintLine { get; set; } = "";

    string ITextAssetSerializable.GetTextAssetSerializedString() => HintLine;

    void ITextAssetSerializable.FromTextAssetSerializedString(string text) => HintLine = text;
}