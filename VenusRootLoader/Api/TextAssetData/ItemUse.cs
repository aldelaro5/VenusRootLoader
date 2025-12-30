using VenusRootLoader.Extensions;
using VenusRootLoader.Patching.Resources.TextAsset;

namespace VenusRootLoader.Api.TextAssetData;

public sealed class ItemUse : ITextAssetSerializable
{
    public MainManager.ItemUsage UseType { get; set; }
    public int Value { get; set; }

    string ITextAssetSerializable.GetTextAssetSerializedString() => $"{UseType},{Value}";

    void ITextAssetSerializable.FromTextAssetSerializedString(string text)
    {
        string[] fields = text.Split(',');
        UseType = Enum.Parse<MainManager.ItemUsage>(fields[0]);
        Value = int.Parse(fields[1]);
    }
}