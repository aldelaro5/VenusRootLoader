using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Resources.TextAsset;

internal interface IOrderingTextAssetParser<TLeaf> where TLeaf : ILeaf
{
    string GetTextAssetString(ILeavesRegistry<TLeaf> registry);
    void FromTextAssetString(string text, ILeavesRegistry<TLeaf> registry);
}