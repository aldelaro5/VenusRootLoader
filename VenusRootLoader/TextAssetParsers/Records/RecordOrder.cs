using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.TextAssetParsers.Records;

internal sealed class RecordOrder : ITextAssetSerializable
{
    internal List<RecordData> OrderedRecordsData { get; } = new();

    string ITextAssetSerializable.GetTextAssetSerializedString() => string.Join("\n", OrderedRecordsData);

    void ITextAssetSerializable.FromTextAssetSerializedString(string text)
    {
        string[] lines = text.Split(StringUtils.NewlineSplitDelimiter);

        OrderedRecordsData.Clear();
        foreach (string line in lines)
        {
            ITextAssetSerializable data = new RecordData();
            data.FromTextAssetSerializedString(line);
            OrderedRecordsData.Add((RecordData)data);
        }
    }
}