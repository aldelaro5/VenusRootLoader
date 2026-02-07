using System.Text;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.TextAssetParsers;

internal sealed class EnemyLocalizedTextAssetParser : ILocalizedTextAssetParser<EnemyLeaf>
{
    public string GetTextAssetSerializedString(string subPath, int languageId, EnemyLeaf leaf)
    {
        StringBuilder sb = new();
        sb.Append(leaf.Name[languageId]);
        sb.Append('@');
        sb.Append(leaf.Biography[languageId]);
        sb.Append('@');
        sb.Append(leaf.BeeSpyDialogue[languageId]);
        sb.Append('@');
        sb.Append(leaf.BeetleSpyDialogue[languageId]);
        sb.Append('@');
        sb.Append(leaf.MothSpyDialogue[languageId]);

        return sb.ToString();
    }

    public void FromTextAssetSerializedString(string subPath, int languageId, string text, EnemyLeaf leaf)
    {
        string[] fields = text.Split(StringUtils.AtSymbolSplitDelimiter);

        leaf.Name[languageId] = fields[0];
        leaf.Biography[languageId] = fields[1];
        leaf.BeeSpyDialogue[languageId] = fields[2];
        leaf.BeetleSpyDialogue[languageId] = fields[3];
        leaf.MothSpyDialogue[languageId] = fields[4];
    }
}