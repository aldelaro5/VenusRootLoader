using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.BaseGameCollector;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;

/// <summary>
/// Handles the conversion of a <see cref="Leaf"/> to its matching <see cref="TextAsset"/> data string and also to fill
/// a <see cref="Leaf"/>'s data given its <see cref="TextAsset"/> data.
/// </summary>
/// <typeparam name="TLeaf">The type of <see cref="Leaf"/> this parser handles conversions with.</typeparam>
internal interface ITextAssetParser<in TLeaf>
    where TLeaf : Leaf
{
    /// <summary>
    /// Allows a <see cref="TextAssetPatcher{TLeaf}"/> to convert a <see cref="Leaf"/> to its <see cref="TextAsset"/> line
    /// string representations.
    /// </summary>
    /// <param name="subPath">The <see cref="TextAsset"/> path excluding the <c>Data/</c> prefix.</param>
    /// <param name="leaf">The leaf to get the string data from.</param>
    /// <returns>The serialized string of the leaf given the <see cref="TextAsset"/> subpath.</returns>
    string GetTextAssetSerializedString(string subPath, TLeaf leaf);

    /// <summary>
    /// Allows an <see cref="IBaseGameCollector"/> to fill in a <see cref="Leaf"/> from a <see cref="TextAsset"/>
    /// string that is associated with the leaf.
    /// </summary>
    /// <param name="subPath">The <see cref="TextAsset"/> path excluding the <c>Data/</c> prefix.</param>
    /// <param name="text">The line data of the <see cref="TextAsset"/> to use.</param>
    /// <param name="leaf">The leaf to fill in data from.</param>
    void FromTextAssetSerializedString(string subPath, string text, TLeaf leaf);
}