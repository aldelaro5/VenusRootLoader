using System.Text;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.TextAssetData.Items;

// TODO: Figure out how to keep internal
public sealed class ItemLanguageData : ITextAssetSerializable
{
    internal string Name { get; set; } = "<NO NAME>";
    private string UnusedDescription { get; set; } = "";
    internal string Description { get; set; } = "<NO DESCRIPTION>";
    internal string? Prepender { get; set; }

    string ITextAssetSerializable.GetTextAssetSerializedString()
    {
        StringBuilder sb = new();
        sb.Append(Name);
        sb.Append('@');
        sb.Append(UnusedDescription);
        sb.Append('@');
        sb.Append(Description);
        if (Prepender != null)
        {
            sb.Append('@');
            sb.Append(Prepender);
        }

        return sb.ToString();
    }

    void ITextAssetSerializable.FromTextAssetSerializedString(string text)
    {
        string[] fields = text.Split(StringUtils.AtSymbolSplitDelimiter);
        Name = fields[0];
        UnusedDescription = fields[1];
        Description = fields[2];
        if (fields.Length > 3)
            Prepender = fields[3];
    }
}