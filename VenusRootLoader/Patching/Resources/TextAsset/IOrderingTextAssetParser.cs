using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Resources.TextAsset;

internal interface IOrderingTextAssetParser<TLeaf> where TLeaf : Leaf
{
    string GetTextAssetString(IOrderedLeavesRegistry<TLeaf> orderedRegistry);
    void FromTextAssetString(string text, IOrderedLeavesRegistry<TLeaf> orderedRegistry);
}