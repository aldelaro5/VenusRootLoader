using System.Text;

namespace VenusRootLoader.Patching.Resources.TextAsset.SerializableData;

internal sealed class ItemLanguageData : ITextAssetSerializable
{
    internal string Name { get; set; } = "<NO NAME>";
    internal string UnusedDescription { get; set; } = "";
    internal string Description { get; set; } = "<NO DESCRIPTION>";
    internal string? Prepender { get; set; }

    public string GetTextAssetSerializedString()
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

    public void FromTextAssetSerializedString(string text)
    {
        string[] fields = text.Split('@');
        Name = fields[0];
        UnusedDescription = fields[1];
        Description = fields[2];
        if (fields.Length > 3)
            Prepender = fields[3];
    }
}