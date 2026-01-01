using VenusRootLoader.Patching.Resources.TextAsset;

namespace VenusRootLoader.Api.TextAssetData.FortuneTellerLines;

public class FortuneTellerCrystalBerryLine : ITextAssetSerializable
{
    public string HintLine { get; set; } = "";

    string ITextAssetSerializable.GetTextAssetSerializedString() => HintLine;

    void ITextAssetSerializable.FromTextAssetSerializedString(string text) => HintLine = text;
}