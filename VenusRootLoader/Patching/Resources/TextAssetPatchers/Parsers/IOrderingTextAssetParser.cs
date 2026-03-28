using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.BaseGameCollector;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;

/// <summary>
/// Handles the conversion of an <see cref="IOrderedLeavesRegistry{TLeaf}"/> to its matching <see cref="TextAsset"/>
/// ordering data string and also to fill an <see cref="IOrderedLeavesRegistry{TLeaf}"/>'s initial ordering given the
/// <see cref="TextAsset"/> ordering data.
/// </summary>
/// <typeparam name="TLeaf">The type of <see cref="Leaf"/> this parser handles ordering data of.</typeparam>
internal interface IOrderingTextAssetParser<TLeaf> where TLeaf : Leaf
{
    /// <summary>
    /// Allows a <see cref="OrderingTextAssetPatcher{TLeaf}"/> to convert a <see cref="IOrderedLeavesRegistry{TLeaf}"/>
    /// ordering data to its <see cref="TextAsset"/> string content.
    /// </summary>
    /// <param name="orderedRegistry">The ordered registry to serialize the data from.</param>
    /// <returns>The serialized <see cref="TextAsset"/> content.</returns>
    string GetTextAssetString(IOrderedLeavesRegistry<TLeaf> orderedRegistry);

    /// <summary>
    /// Allows an <see cref="IBaseGameCollector"/> to fill in an <see cref="IOrderedLeavesRegistry{TLeaf}"/>'s initial
    /// ordering data from a <see cref="TextAsset"/> string that is associated with the registry.
    /// </summary>
    /// <param name="text">The <see cref="TextAsset"/> content to use.</param>
    /// <param name="orderedRegistry">The ordered registry to set its initial ordering data.</param>
    void FromTextAssetString(string text, IOrderedLeavesRegistry<TLeaf> orderedRegistry);
}