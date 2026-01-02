using System.Text;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.TextAssetParsers.SpyCards;

internal sealed class SpyCardData : ITextAssetSerializable
{
    internal int TpCost { get; set; }
    internal int Attack { get; set; }
    internal int EnemyGameId { get; set; }
    private float UnusedHorizontalNameSize { get; set; } = 1.0f;
    internal CardGame.Type Type { get; set; }
    internal List<SpyCardEffect> Effects { get; } = new();
    internal List<CardGame.Tribe> Tribes { get; } = new();

    string ITextAssetSerializable.GetTextAssetSerializedString()
    {
        StringBuilder sb = new();
        sb.Append(TpCost);
        sb.Append(',');
        sb.Append(Attack);
        sb.Append(',');
        sb.Append(EnemyGameId);
        sb.Append(',');
        sb.Append(UnusedHorizontalNameSize);
        sb.Append(',');
        sb.Append((int)Type);
        sb.Append(',');

        string[] serializedEffects = Effects
            .Cast<ITextAssetSerializable>()
            .Select(e => e.GetTextAssetSerializedString())
            .ToArray();
        sb.Append(string.Join("@", serializedEffects));

        sb.Append(',');
        sb.Append(string.Join("@", Tribes.Cast<int>()));

        return sb.ToString();
    }

    void ITextAssetSerializable.FromTextAssetSerializedString(string text)
    {
        string[] fields = text.Split(StringUtils.CommaSplitDelimiter);
        TpCost = int.Parse(fields[0]);
        Attack = int.Parse(fields[1]);
        EnemyGameId = int.Parse(fields[2]);
        UnusedHorizontalNameSize = float.Parse(fields[3]);
        Type = (CardGame.Type)int.Parse(fields[4]);

        string[] effects = fields[5].Split(StringUtils.AtSymbolSplitDelimiter);
        Effects.Clear();
        foreach (string effect in effects)
        {
            SpyCardEffect spyCardEffect = new();
            ((ITextAssetSerializable)spyCardEffect).FromTextAssetSerializedString(effect);
            Effects.Add(spyCardEffect);
        }

        string[] tribes = fields[6].Split(StringUtils.AtSymbolSplitDelimiter);
        Tribes.Clear();
        Tribes.AddRange(tribes.Select(int.Parse).Cast<CardGame.Tribe>());
    }
}