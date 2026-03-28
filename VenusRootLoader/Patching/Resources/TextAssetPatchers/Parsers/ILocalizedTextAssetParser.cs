using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.BaseGameCollector;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;

/// <summary>
/// Handles the conversion of a <see cref="Leaf"/> to its matching localized <see cref="TextAsset"/> data string and
/// also to fill a <see cref="Leaf"/>'s data given its localized <see cref="TextAsset"/> data.
/// </summary>
/// <typeparam name="TLeaf">The type of <see cref="Leaf"/> this parser handles conversions with.</typeparam>
internal interface ILocalizedTextAssetParser<in TLeaf>
    where TLeaf : Leaf
{
    /// <summary>
    /// Allows a <see cref="LocalizedTextAssetPatcher{TLeaf}"/> to convert a <see cref="Leaf"/> to its localized
    /// <see cref="TextAsset"/> line string representations.
    /// </summary>
    /// <param name="subPath">The <see cref="TextAsset"/> path excluding the <c>Data/DialoguesX</c> prefix where X is the <paramref name="languageId"/>.</param>
    /// <param name="languageId">The language game id associated with the data to serialize.</param>
    /// <param name="leaf">The leaf to get the string data from.</param>
    /// <returns>The serialized string of the leaf given the localized <see cref="TextAsset"/> subpath.</returns>
    string GetTextAssetSerializedString(string subPath, int languageId, TLeaf leaf);

    /// <summary>
    /// Allows an <see cref="IBaseGameCollector"/> to fill in a <see cref="Leaf"/> from a localized <see cref="TextAsset"/>
    /// string that is associated with the leaf.
    /// </summary>
    /// <param name="subPath">The localized <see cref="TextAsset"/> path excluding the <c>Data/DialoguesX</c> prefix where X is the <paramref name="languageId"/>.</param>
    /// <param name="languageId">The language game id associated with the localized data.</param>
    /// <param name="text">The line data of the localized <see cref="TextAsset"/> to use.</param>
    /// <param name="leaf">The leaf to fill in data from.</param>
    void FromTextAssetSerializedString(string subPath, int languageId, string text, TLeaf leaf);
}