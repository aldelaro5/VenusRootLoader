using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.TextAssetData.Discoveries;

internal sealed class DiscoveryOrder : ITextAssetSerializable
{
    internal List<DiscoveryData> OrderedDiscoveriesData { get; } = new();

    string ITextAssetSerializable.GetTextAssetSerializedString() => string.Join("\n", OrderedDiscoveriesData);

    void ITextAssetSerializable.FromTextAssetSerializedString(string text)
    {
        string[] lines = text.Split(StringUtils.NewlineSplitDelimiter);
        OrderedDiscoveriesData.Clear();
        foreach (string line in lines)
        {
            ITextAssetSerializable data = new DiscoveryData();
            data.FromTextAssetSerializedString(line);
            OrderedDiscoveriesData.Add((DiscoveryData)data);
        }
    }
}