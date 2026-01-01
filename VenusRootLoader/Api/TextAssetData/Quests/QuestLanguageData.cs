using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Api.TextAssetData.Quests;

public sealed class QuestLanguageData : ITextAssetSerializable
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Sender { get; set; } = "";

    string ITextAssetSerializable.GetTextAssetSerializedString() => $"{Name}@{Description}@{Sender}";

    void ITextAssetSerializable.FromTextAssetSerializedString(string text)
    {
        string[] fields = text.Split(StringUtils.AtSymbolSplitDelimiter);

        Name = fields[0];
        Description = fields[1];
        Sender = fields[2];
    }
}