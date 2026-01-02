using System.Text;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.TextAssetData.Enemies;

internal sealed class EnemyLanguageData : ITextAssetSerializable
{
    internal string Name { get; set; } = "<NO NAME>";
    internal string Biography { get; set; } = "biotattle";
    internal string BeeSpyDialogue { get; set; } = "beetattle";
    internal string BeetleSpyDialogue { get; set; } = "beetleattle";
    internal string MothSpyDialogue { get; set; } = "mothtattle";

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