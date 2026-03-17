using Microsoft.Extensions.Logging;
using System.Text;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers;

internal interface IMapEntityTextAssetPatcher
{
    TextAsset PatchMapEntityTextAsset(string path, TextAsset original);
}

internal sealed class MapEntitiesTextAssetPatcher : IMapEntityTextAssetPatcher
{
    private readonly ILogger<MapEntitiesTextAssetPatcher> _logger;
    private readonly ILeavesRegistry<MapLeaf> _mapsRegistry;
    private readonly ITextAssetParser<MapLeaf.MapEntity> _parser;

    public MapEntitiesTextAssetPatcher(
        ILogger<MapEntitiesTextAssetPatcher> logger,
        ILeavesRegistry<MapLeaf> mapsRegistry,
        ITextAssetParser<MapLeaf.MapEntity> parser)
    {
        _logger = logger;
        _parser = parser;
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
            int mapGameIdEnd = path.Length - "names".Length - 1;
            mapGameId = int.Parse(path.Substring(mapGameIdStart, mapGameIdEnd - mapGameIdStart + 1));
        }
        else
        {
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
#pragma warning disable IO0002
#pragma warning disable IO0006
#pragma warning disable IO0003
        {
            string dumpPath = Path.Combine(Directory.GetCurrentDirectory(), path.ToLower() + ".txt");
            Directory.CreateDirectory(Path.GetDirectoryName(dumpPath));
            File.WriteAllText(dumpPath, text);
        }
#pragma warning restore IO0003
#pragma warning restore IO0006
#pragma warning restore IO0002
        return new TextAsset(text);
    }
}