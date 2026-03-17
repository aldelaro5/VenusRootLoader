using Microsoft.Extensions.Logging;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers;

internal interface IMapDialoguesTextAssetPatcher
{
    TextAsset PatchMapDialoguesTextAsset(string path, int languageId, TextAsset original);
}

internal sealed class MapDialoguesTextAssetPatcher : IMapDialoguesTextAssetPatcher
{
    private readonly ILogger<MapDialoguesTextAssetPatcher> _logger;
    private readonly ILeavesRegistry<MapLeaf> _mapsRegistry;

    public MapDialoguesTextAssetPatcher(
        ILogger<MapDialoguesTextAssetPatcher> logger,
        ILeavesRegistry<MapLeaf> mapsRegistry)
    {
        _logger = logger;
        _mapsRegistry = mapsRegistry;
    }

    public TextAsset PatchMapDialoguesTextAsset(string path, int languageId, TextAsset original)
    {
        bool registryHasData = _mapsRegistry.LeavesByNamedIds.Count > 0;
        if (!registryHasData)
            return original;

        int mapNameStart = path.LastIndexOf('/') + 1;
        string mapName = path[mapNameStart..];

        MapLeaf leaf = _mapsRegistry.LeavesByNamedIds[mapName];
        List<string> newLines = leaf.Dialogues[languageId];

        string text = string.Join("\n", newLines);
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