using Microsoft.Extensions.Logging;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers;

internal interface IMapDialoguesTextAssetPatcher
{
    TextAsset PatchMapDialoguesTextAsset(int languageId, string path, TextAsset original);
}

internal sealed class MapDialoguesTextAssetPatcher : IMapDialoguesTextAssetPatcher
{
    private readonly ILogger<MapDialoguesTextAssetPatcher> _logger;
    private readonly ITextAssetDumper _textAssetDumper;
    private readonly ILeavesRegistry<MapLeaf> _mapsRegistry;

    public MapDialoguesTextAssetPatcher(
        ILogger<MapDialoguesTextAssetPatcher> logger,
        ITextAssetDumper textAssetDumper,
        ILeavesRegistry<MapLeaf> mapsRegistry)
    {
        _logger = logger;
        _mapsRegistry = mapsRegistry;
        _textAssetDumper = textAssetDumper;
    }

    public TextAsset PatchMapDialoguesTextAsset(int languageId, string path, TextAsset original)
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
            _textAssetDumper.DumpTextAssetContent(path, text);

        return new TextAsset(text);
    }
}