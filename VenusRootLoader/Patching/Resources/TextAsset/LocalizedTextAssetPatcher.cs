using Microsoft.Extensions.Logging;
using System.Text;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Resources.TextAsset;

internal interface ILocalizedTextAssetPatcher
{
    string[] SubPaths { get; }
    UnityEngine.TextAsset PatchLocalisedTextAsset(int languageId, string subpath, UnityEngine.TextAsset original);
}

internal sealed class LocalizedTextAssetPatcher<T> : ILocalizedTextAssetPatcher
    where T : ILeaf
{
    private readonly Func<ILeavesRegistry<T>, IEnumerable<T>>? _leavesSorter;
    private readonly ILeavesRegistry<T> _registry;
    private readonly ILogger<LocalizedTextAssetPatcher<T>> _logger;
    private readonly ILocalizedTextAssetParser<T> _parser;

    public LocalizedTextAssetPatcher(
        string[] subPaths,
        Func<ILeavesRegistry<T>, IEnumerable<T>>? leavesSorter,
        ILogger<LocalizedTextAssetPatcher<T>> logger,
        ILeavesRegistry<T> registry,
        ILocalizedTextAssetParser<T> parser)
    {
        SubPaths = subPaths;
        _leavesSorter = leavesSorter;
        _registry = registry;
        _logger = logger;
        _parser = parser;
    }

    public string[] SubPaths { get; }

    public UnityEngine.TextAsset PatchLocalisedTextAsset(int languageId, string subpath, UnityEngine.TextAsset original)
    {
        bool registryHasData = _registry.LeavesByNamedIds.Count > 0;

        if (!registryHasData)
            return original;

        IEnumerable<T> sortedLeaves = _leavesSorter is null
            ? _registry.LeavesByGameIds.Values.OrderBy(l => l.GameId)
            : _leavesSorter(_registry);
        IEnumerable<string> newLines = sortedLeaves
            .Select(customLine => _parser.GetTextAssetSerializedString(subpath, languageId, customLine));

        // Some game data relies on having a trailing LF for the parsing to work correctly
        StringBuilder sb = new(string.Join("\n", newLines));
        if (original.text.EndsWith("\n"))
            sb.Append('\n');

        string text = sb.ToString();
        _logger.LogTrace("Patching {path} for language {language}:\n{text}", subpath, languageId, text);
        return new UnityEngine.TextAsset(text);
    }
}