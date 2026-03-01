using CommunityToolkit.Diagnostics;
using System.Text;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.TextAsset.Parsers.GlobalData;

internal sealed class RecipeLibraryEntryTextAssetParser : ITextAssetParser<RecipeLibraryEntryLeaf>
{
    private const string CookOrderSubPath = "CookOrder";
    private const string CookLibrarySubPath = "CookLibrary";

    private readonly ILeavesRegistry<ItemLeaf> _itemsRegistry;

    public RecipeLibraryEntryTextAssetParser(ILeavesRegistry<ItemLeaf> itemsRegistry)
    {
        _itemsRegistry = itemsRegistry;
    }

    public string GetTextAssetSerializedString(string subPath, RecipeLibraryEntryLeaf leaf)
    {
        if (subPath.Equals(CookOrderSubPath, StringComparison.OrdinalIgnoreCase))
            return leaf.Recipe.Leaf.ResultItem.GameId.ToString();

        if (!subPath.Equals(CookLibrarySubPath, StringComparison.OrdinalIgnoreCase))
            return ThrowHelper.ThrowInvalidDataException<string>($"This parser doesn't support the subPath {subPath}");

        StringBuilder sb = new();
        if (leaf.Recipe.Leaf.FirstItem is null &&
            leaf.Recipe.Leaf.SecondItem is null)
        {
            sb.Append("-1@");
            return sb.ToString();
        }

        sb.Append(leaf.Recipe.Leaf.FirstItem!.Value.GameId);
        if (leaf.Recipe.Leaf.SecondItem is not null)
        {
            sb.Append(',');
            sb.Append(leaf.Recipe.Leaf.SecondItem.Value.GameId);
        }

        sb.Append('@');
        return sb.ToString();
    }

    public void FromTextAssetSerializedString(string subPath, string text, RecipeLibraryEntryLeaf leaf)
    {
        if (subPath.Equals(CookOrderSubPath, StringComparison.OrdinalIgnoreCase))
            leaf.Recipe.Leaf.ResultItem = new(_itemsRegistry.LeavesByGameIds[int.Parse(text)]);
        else if (subPath.Equals(CookLibrarySubPath, StringComparison.OrdinalIgnoreCase))
        {
            string[] fields = text.Replace("@", "").Split(StringUtils.CommaSplitDelimiter);
            int firstItem = int.Parse(fields[0]);
            if (firstItem == -1)
            {
                leaf.Recipe.Leaf.FirstItem = null;
                leaf.Recipe.Leaf.SecondItem = null;
                return;
            }

            leaf.Recipe.Leaf.FirstItem = new(_itemsRegistry.LeavesByGameIds[firstItem]);
            if (fields.Length > 1)
                leaf.Recipe.Leaf.SecondItem = new(_itemsRegistry.LeavesByGameIds[int.Parse(fields[1])]);
        }
        else
        {
            ThrowHelper.ThrowInvalidDataException($"This parser doesn't support the subPath {subPath}");
        }
    }
}