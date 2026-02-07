using System.Text;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.TextAsset.Parsers;

// TODO: Handle ordering which is game id, one per line
internal sealed class SpyCardTextAssetParser : ITextAssetParser<SpyCardLeaf>
{
    public string GetTextAssetSerializedString(string subPath, SpyCardLeaf leaf)
    {
        StringBuilder sb = new();
        sb.Append(leaf.TpCost);
        sb.Append(',');
        sb.Append(leaf.Attack);
        sb.Append(',');
        sb.Append(leaf.EnemyGameId);
        sb.Append(',');
        sb.Append(leaf.UnusedHorizontalNameSize);
        sb.Append(',');
        sb.Append((int)leaf.Type);
        sb.Append(',');

        IEnumerable<string> serializedEffects = leaf.Effects
            .Select(e => $"{(int)e.Effect}#{e.FirstValue}#{e.SecondValue}");
        sb.Append(string.Join("@", serializedEffects));

        sb.Append(',');
        sb.Append(string.Join("@", leaf.Tribes.Cast<int>()));

        return sb.ToString();
    }

    public void FromTextAssetSerializedString(string subPath, string text, SpyCardLeaf leaf)
    {
        string[] fields = text.Split(StringUtils.CommaSplitDelimiter);
        leaf.TpCost = int.Parse(fields[0]);
        leaf.Attack = int.Parse(fields[1]);
        leaf.EnemyGameId = int.Parse(fields[2]);
        leaf.UnusedHorizontalNameSize = float.Parse(fields[3]);
        leaf.Type = (CardGame.Type)int.Parse(fields[4]);

        string[] effects = fields[5].Split(StringUtils.AtSymbolSplitDelimiter);
        leaf.Effects.Clear();
        foreach (string effect in effects)
        {
            SpyCardLeaf.SpyCardEffect cardEffect = new();
            string[] fieldsEffect = effect.Split(StringUtils.NumberSignSplitDelimiter);
            cardEffect.Effect = (CardGame.Effects)int.Parse(fieldsEffect[0]);
            cardEffect.FirstValue = int.Parse(fieldsEffect[1]);
            cardEffect.SecondValue = int.Parse(fieldsEffect[2]);
            leaf.Effects.Add(cardEffect);
        }

        string[] tribes = fields[6].Split(StringUtils.AtSymbolSplitDelimiter);
        leaf.Tribes.Clear();
        leaf.Tribes.AddRange(tribes.Select(int.Parse).Cast<CardGame.Tribe>());
    }
}