using UnityEngine;
using VenusRootLoader.Api.MapEntities;
using VenusRootLoader.BaseGameCollector;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;

/// <summary>
/// Handles the conversion of a <see cref="MapEntity"/> to its matching <see cref="TextAsset"/> data or name string and
/// also to fill a <see cref="MapEntity"/>'s data given its <see cref="TextAsset"/> data or name.
/// </summary>
public interface IMapEntityTextAssetParser
{
    /// <summary>
    /// Allows the <see cref="MapEntitiesTextAssetPatcher"/> to convert a <see cref="MapEntity"/> to its <see cref="TextAsset"/> line
    /// string representations in its name or data.
    /// </summary>
    /// <param name="subPath">The <see cref="TextAsset"/> path excluding the <c>Data/EntityData/</c> prefix.</param>
    /// <param name="mapEntity">The map entity to get the string data from.</param>
    /// <returns>The serialized string of the <see cref="MapEntity"/> given the <see cref="TextAsset"/> subpath.</returns>
    string GetTextAssetSerializedString(string subPath, MapEntity mapEntity);

    /// <summary>
    /// Allows an <see cref="IBaseGameCollector"/> to fill in a <see cref="MapEntity"/> from a <see cref="TextAsset"/>
    /// string that is associated with its data or name.
    /// </summary>
    /// <param name="id">The associated <see cref="MapEntity"/>'s unique id for the map.</param>
    /// <param name="name">The <see cref="MapEntity"/>'s name.</param>
    /// <param name="text">The line data of the <see cref="TextAsset"/> to use.</param>
    MapEntity FromTextAssetSerializedString(int id, string name, string text);
}