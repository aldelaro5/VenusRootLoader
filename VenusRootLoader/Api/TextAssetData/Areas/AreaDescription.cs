using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Api.TextAssetData.Areas;

public sealed class AreaDescription : ITextAssetSerializable
{
    public List<string> PaginatedDescription { get; } = new();

    string ITextAssetSerializable.GetTextAssetSerializedString() => string.Join("{", PaginatedDescription);

    void ITextAssetSerializable.FromTextAssetSerializedString(string text)
    {
        string[] pages = text.Split(StringUtils.OpeningBraceSplitDelimiter);

        PaginatedDescription.Clear();
        foreach (string page in pages)
            PaginatedDescription.Add(page);
    }
}