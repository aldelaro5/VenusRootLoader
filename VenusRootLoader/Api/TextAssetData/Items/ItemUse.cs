using VenusRootLoader.Extensions;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Api.TextAssetData.Items;

public sealed class ItemUse : ITextAssetSerializable
{
    public MainManager.ItemUsage UseType { get; set; }
    public int Value { get; set; }

    string ITextAssetSerializable.GetTextAssetSerializedString() => $"{UseType},{Value}";

    void ITextAssetSerializable.FromTextAssetSerializedString(string text)
    {
        string[] fields = text.Split(StringUtils.CommaSplitDelimiter);
        UseType = Enum.Parse<MainManager.ItemUsage>(fields[0]);
        Value = int.Parse(fields[1]);
    }
}