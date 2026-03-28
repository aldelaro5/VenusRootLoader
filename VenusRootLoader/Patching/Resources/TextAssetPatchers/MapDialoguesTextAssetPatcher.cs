using Microsoft.Extensions.Logging;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers;

/// <summary>
/// A patcher that handles patching localized line based <see cref="TextAsset"/> given that the resources path after
/// the Dialogues directory starts with Maps. Maps dialogues aren't <see cref="Leaf"/> because there's multiple lines per
/// <see cref="MapLeaf"/> and they belong to each map rather than exist independently. It assumes the specific naming
/// structure of the maps dialogues <see cref="TextAsset"/>.
/// </summary>
internal interface IMapDialoguesTextAssetPatcher
{
    /// <summary>
    /// Patches the original <see cref="TextAsset"/> given that the game requested to load it using the resources path
    /// <paramref name="path"/> excluding the <c>Data/DialoguesX/</c> prefix where X is <paramref name="languageId"/>.
    /// It is assumed that all <see cref="TextAsset"/> are assets within the Maps subdirectory in the DialgouesX directory.
    /// </summary>
    /// <param name="languageId">The language game id the localized <see cref="TextAsset"/> is associated with.</param>
    /// <param name="path">The resources path the game requested to load excluding the <c>Data/DialoguesX/</c> prefix.</param>
    /// <param name="original">The original <see cref="TextAsset"/> that would be returned if the patcher wasn't present.</param>
    /// <returns>The patched <see cref="TextAsset"/>.</returns>
    TextAsset PatchMapDialoguesTextAsset(int languageId, string path, TextAsset original);
}

/// <inheritdoc/>
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