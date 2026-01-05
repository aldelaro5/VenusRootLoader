using CommunityToolkit.Diagnostics;
using System.Text;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.TextAssetParsers;

internal sealed class RecipeLibraryEntryTextAssetPatcher : ITextAssetSerializable<RecipeLibraryEntryLeaf>
{
    private const string CookOrderSubPath = "CookOrder";
    private const string CookLibrarySubPath = "CookLibrary";

    public string GetTextAssetSerializedString(string subPath, RecipeLibraryEntryLeaf leaf)
    {
        if (subPath.Equals(CookOrderSubPath, StringComparison.OrdinalIgnoreCase))
            return leaf.ResultItemGameId.ToString();

        if (!subPath.Equals(CookLibrarySubPath, StringComparison.OrdinalIgnoreCase))
            return ThrowHelper.ThrowInvalidDataException<string>($"This parser doesn't support the subPath {subPath}");

        StringBuilder sb = new();
        sb.Append(leaf.FirstItemGameId);
        if (leaf.SecondItemGameId is not null)
        {
            sb.Append(',');
            sb.Append(leaf.SecondItemGameId.Value.ToString());
        }

        sb.Append('@');
        return sb.ToString();
    }

    public void FromTextAssetSerializedString(string subPath, string text, RecipeLibraryEntryLeaf leaf)
    {
        if (subPath.Equals(CookOrderSubPath, StringComparison.OrdinalIgnoreCase))
            leaf.ResultItemGameId = int.Parse(text);
        else if (subPath.Equals(CookLibrarySubPath, StringComparison.OrdinalIgnoreCase))
        {
            string[] fields = text.Remove('@').Split(StringUtils.CommaSplitDelimiter);
            leaf.FirstItemGameId = int.Parse(fields[0]);
            if (fields.Length > 1)
                leaf.SecondItemGameId = int.Parse(fields[1]);
        }
        else
        {
            ThrowHelper.ThrowInvalidDataException($"This parser doesn't support the subPath {subPath}");
        }
    }
}