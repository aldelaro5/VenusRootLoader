using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Api.Leaves.MapEntities;
using VenusRootLoader.BaseGameCollector;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;

/// <summary>
/// Handles the conversion of a <see cref="MapEntityLeaf"/> to its matching <see cref="TextAsset"/> data or name string and
/// also to fill a <see cref="MapEntityLeaf"/>'s data given its <see cref="TextAsset"/> data or name.
/// </summary>
internal interface IMapEntityTextAssetParser
{
    /// <summary>
    /// Allows the <see cref="MapEntitiesTextAssetPatcher"/> to convert a <see cref="MapEntityLeaf"/> to its <see cref="TextAsset"/> line
    /// string representations in its name or data.
    /// </summary>
    /// <param name="subPath">The <see cref="TextAsset"/> path excluding the <c>Data/EntityData/</c> prefix.</param>
    /// <param name="mapEntityLeaf">The map entity to get the string data from.</param>
    /// <returns>The serialized string of the <see cref="MapEntityLeaf"/> given the <see cref="TextAsset"/> subpath.</returns>
    string GetTextAssetSerializedString(string subPath, MapEntityLeaf mapEntityLeaf);

    /// <summary>
    /// Allows an <see cref="IBaseGameCollector"/> to fill in a <see cref="MapEntityLeaf"/> from a <see cref="TextAsset"/>
    /// string that is associated with its data or name.
    /// </summary>
    /// <param name="map">The <see cref="MapLeaf"/> associated with the <see cref="MapEntityLeaf"/>.</param>
    /// <param name="baseGameId">The <see cref="Leaf.CreatorId"/> of the base game to use when registering the entity to the map's entities registry</param>
    /// <param name="id">The associated <see cref="MapEntityLeaf"/>'s unique id for the map.</param>
    /// <param name="name">The <see cref="MapEntityLeaf"/>'s name.</param>
    /// <param name="text">The line data of the <see cref="TextAsset"/> to use.</param>
    /// <remarks>This will register the entity to the map's entity registry using its parsed type</remarks>
    void FromTextAssetSerializedString(MapLeaf map, string baseGameId, int id, string name, string text);
}