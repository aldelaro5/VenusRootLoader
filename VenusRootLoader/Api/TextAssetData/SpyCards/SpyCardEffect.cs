using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Api.TextAssetData.SpyCards;

public sealed class SpyCardEffect : ITextAssetSerializable
{
    public CardGame.Effects Effect { get; set; }
    public int FirstValue { get; set; }
    public int SecondValue { get; set; }

    string ITextAssetSerializable.GetTextAssetSerializedString() => $"{(int)Effect}#{FirstValue}#{SecondValue}";

    void ITextAssetSerializable.FromTextAssetSerializedString(string text)
    {
        string[] fields = text.Split(StringUtils.NumberSignSplitDelimiter);
        Effect = (CardGame.Effects)int.Parse(fields[0]);
        FirstValue = int.Parse(fields[1]);
        SecondValue = int.Parse(fields[2]);
    }
}