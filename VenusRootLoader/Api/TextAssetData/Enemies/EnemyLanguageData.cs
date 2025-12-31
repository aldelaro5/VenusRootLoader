using System.Text;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Api.TextAssetData.Enemies;

public sealed class EnemyLanguageData : ITextAssetSerializable
{
    public string Name { get; set; } = "<NO NAME>";
    public string Biography { get; set; } = "biotattle";
    public string BeeSpyDialogue { get; set; } = "beetattle";
    public string BeetleSpyDialogue { get; set; } = "beetleattle";
    public string MothSpyDialogue { get; set; } = "mothtattle";

    string ITextAssetSerializable.GetTextAssetSerializedString()
    {
        StringBuilder sb = new();
        sb.Append(Name);
        sb.Append('@');
        sb.Append(Biography);
        sb.Append('@');
        sb.Append(BeeSpyDialogue);
        sb.Append('@');
        sb.Append(BeetleSpyDialogue);
        sb.Append('@');
        sb.Append(MothSpyDialogue);

        return sb.ToString();
    }

    void ITextAssetSerializable.FromTextAssetSerializedString(string text)
    {
        string[] fields = text.Split(StringUtils.AtSymbolSplitDelimiter);

        Name = fields[0];
        Biography = fields[1];
        BeeSpyDialogue = fields[2];
        BeetleSpyDialogue = fields[3];
        MothSpyDialogue = fields[4];
    }
}