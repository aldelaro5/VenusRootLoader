using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Api.TextAssetData.Quests;

public sealed class QuestPrerequisite : ITextAssetSerializable
{
    public List<int> RequiredFlagIds { get; } = new();

    string ITextAssetSerializable.GetTextAssetSerializedString() => string.Join("@", RequiredFlagIds);

    void ITextAssetSerializable.FromTextAssetSerializedString(string text)
    {
        string[] fields = text.Split(StringUtils.AtSymbolSplitDelimiter);

        RequiredFlagIds.Clear();
        foreach (string field in fields)
            RequiredFlagIds.Add(int.Parse(field));
    }
}