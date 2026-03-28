using Microsoft.Extensions.Logging;
using System.Text;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Api.MapEntities;
using VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers;

/// <summary>
/// A patcher that handles patching line based <see cref="TextAsset"/> given that the resources path is inside
/// <c>Data/EntityData/</c>. This assumes the specific directory structure of maps entity data.
/// </summary>
internal interface IMapEntityTextAssetPatcher
{
    /// <summary>
    /// Patches the original <see cref="TextAsset"/> given that the game requested to load it using the resources path
    /// <paramref name="path"/> excluding the <c>Data/EntityData/</c> prefix. It is assumed that all <see cref="TextAsset"/>
    /// are assets within the <c>Data/EntityData/</c> directory.
    /// </summary>
    /// <param name="path">The resources path the game requested to load excluding the <c>Data/EntityData/</c> prefix.</param>
    /// <param name="original">The original <see cref="TextAsset"/> that would be returned if the patcher wasn't present.</param>
    /// <returns>The patched <see cref="TextAsset"/>.</returns>
    TextAsset PatchMapEntityTextAsset(string path, TextAsset original);
}

/// <inheritdoc/>
/// This relies on a <see cref="IMapEntityTextAssetParser"/> to do the conversion from <see cref="MapEntity"/> to string.
internal sealed class MapEntitiesTextAssetPatcher : IMapEntityTextAssetPatcher
{
    private readonly ILogger<MapEntitiesTextAssetPatcher> _logger;
    private readonly ITextAssetDumper _textAssetDumper;
    private readonly ILeavesRegistry<MapLeaf> _mapsRegistry;
    private readonly IMapEntityTextAssetParser _parser;

    public MapEntitiesTextAssetPatcher(
        ILogger<MapEntitiesTextAssetPatcher> logger,
        ITextAssetDumper textAssetDumper,
        ILeavesRegistry<MapLeaf> mapsRegistry,
        IMapEntityTextAssetParser parser)
    {
        _logger = logger;
        _parser = parser;
        _textAssetDumper = textAssetDumper;
        _mapsRegistry = mapsRegistry;
    }

    public TextAsset PatchMapEntityTextAsset(string path, TextAsset original)
    {
        bool registryHasData = _mapsRegistry.LeavesByNamedIds.Count > 0;
        if (!registryHasData)
            return original;

        int mapGameIdStart = path.LastIndexOf('/') + 1;
        int mapGameId;
        if (path.EndsWith("names", StringComparison.OrdinalIgnoreCase))
        {
            // Need to extract the map game id which is a prefix to the word "names" of the asset's filename.
            int mapGameIdEnd = path.Length - "names".Length - 1;
            mapGameId = int.Parse(path.Substring(mapGameIdStart, mapGameIdEnd - mapGameIdStart + 1));
        }
        else
        {
            // Here, the map game id is simply the asset's filename.
            mapGameId = int.Parse(path.Substring(mapGameIdStart, path.Length - mapGameIdStart));
        }

        MapLeaf leaf = _mapsRegistry.LeavesByGameIds[mapGameId];
        IEnumerable<string> newLines = leaf.Entities
            .Select(mapEntity => _parser.GetTextAssetSerializedString(path, mapEntity));

        // Some game data relies on having a trailing LF for the parsing to work correctly
        StringBuilder sb = new(string.Join("\n", newLines));
        if (original.text.EndsWith("\n"))
            sb.Append('\n');

        string text = sb.ToString();
        if (_logger.IsEnabled(LogLevel.Trace))
            _textAssetDumper.DumpTextAssetContent(path, text);

        return new TextAsset(text);
    }
}