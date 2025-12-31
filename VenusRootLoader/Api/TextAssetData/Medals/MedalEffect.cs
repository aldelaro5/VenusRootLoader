using VenusRootLoader.Extensions;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Api.TextAssetData.Medals;

public sealed class MedalEffect : ITextAssetSerializable
{
    public MainManager.BadgeEffects Effect { get; set; }
    public int Value { get; set; }

    string ITextAssetSerializable.GetTextAssetSerializedString() => $"{Effect},{Value}";

    void ITextAssetSerializable.FromTextAssetSerializedString(string text)
    {
        string[] fields = text.Split(StringUtils.CommaSplitDelimiter);
        Effect = Enum.Parse<MainManager.BadgeEffects>(fields[0]);
        Value = int.Parse(fields[1]);
    }
}