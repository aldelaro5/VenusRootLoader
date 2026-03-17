using System.Text;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers.GlobalData;

internal sealed class SpyCardTextAssetParser : ITextAssetParser<SpyCardLeaf>
{
    private readonly ILeavesRegistry<EnemyLeaf> _enemiesRegistry;

    public SpyCardTextAssetParser(ILeavesRegistry<EnemyLeaf> enemiesRegistry)
    {
        _enemiesRegistry = enemiesRegistry;
    }

    public string GetTextAssetSerializedString(string subPath, SpyCardLeaf value)
    {
        StringBuilder sb = new();
        sb.Append(value.TpCost);
        sb.Append(',');
        sb.Append(value.Attack);
        sb.Append(',');
        sb.Append(value.Enemy.GameId);
        sb.Append(',');
        sb.Append(value.UnusedHorizontalNameSize);
        sb.Append(',');
        sb.Append((int)value.Type);
        sb.Append(',');

        IEnumerable<string> serializedEffects = value.Effects
            .Select(e => $"{(int)e.Effect}#{e.FirstValue}#{e.SecondValue}");
        sb.Append(string.Join("@", serializedEffects));

        sb.Append(',');
        sb.Append(string.Join("@", value.Tribes.Cast<int>()));

        return sb.ToString();
    }

    public void FromTextAssetSerializedString(string subPath, string text, SpyCardLeaf value)
    {
        string[] fields = text.Split(StringUtils.CommaSplitDelimiter);
        value.TpCost = int.Parse(fields[0]);
        value.Attack = int.Parse(fields[1]);
        value.Enemy = new(_enemiesRegistry.LeavesByGameIds[int.Parse(fields[2])]);
        value.UnusedHorizontalNameSize = float.Parse(fields[3]);
        value.Type = (CardGame.Type)int.Parse(fields[4]);

        string[] effects = fields[5].Split(StringUtils.AtSymbolSplitDelimiter);
        value.Effects.Clear();
        foreach (string effect in effects)
        {
            SpyCardLeaf.SpyCardEffect cardEffect = new();
            string[] fieldsEffect = effect.Split(StringUtils.NumberSignSplitDelimiter);
            if (string.IsNullOrWhiteSpace(fieldsEffect[0]))
                continue;
            cardEffect.Effect = (CardGame.Effects)int.Parse(fieldsEffect[0]);
            cardEffect.FirstValue = int.Parse(fieldsEffect[1]);
            cardEffect.SecondValue = int.Parse(fieldsEffect[2]);
            value.Effects.Add(cardEffect);
        }

        string[] tribes = fields[6].Split(StringUtils.AtSymbolSplitDelimiter);
        value.Tribes.Clear();
        value.Tribes.AddRange(tribes.Select(int.Parse).Cast<CardGame.Tribe>());
    }
}