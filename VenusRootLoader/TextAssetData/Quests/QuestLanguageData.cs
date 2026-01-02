using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.TextAssetData.Quests;

internal sealed class QuestLanguageData : ITextAssetSerializable
{
    internal string Name { get; set; } = "";
    internal string Description { get; set; } = "";
    internal string Sender { get; set; } = "";

    string ITextAssetSerializable.GetTextAssetSerializedString() => $"{Name}@{Description}@{Sender}";

    void ITextAssetSerializable.FromTextAssetSerializedString(string text)
    {
        string[] fields = text.Split(StringUtils.AtSymbolSplitDelimiter);

        Name = fields[0];
        Description = fields[1];
        Sender = fields[2];
    }
}