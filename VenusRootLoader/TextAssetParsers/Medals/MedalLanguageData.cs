using System.Text;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.TextAssetParsers.Medals;

internal sealed class MedalLanguageData : ITextAssetSerializable
{
    internal string Name { get; set; } = "<NO NAME>";
    internal string Description { get; set; } = "<NO DESCRIPTION>";
    internal string Prepender { get; set; } = "";

    string ITextAssetSerializable.GetTextAssetSerializedString()
    {
        StringBuilder sb = new();
        sb.Append(Name);
        sb.Append('@');
        sb.Append(Description);
        sb.Append('@');
        sb.Append(Prepender);

        return sb.ToString();
    }

    void ITextAssetSerializable.FromTextAssetSerializedString(string text)
    {
        string[] fields = text.Split(StringUtils.AtSymbolSplitDelimiter);
        Name = fields[0];
        Description = fields[1];
        Prepender = fields[2];
    }
}