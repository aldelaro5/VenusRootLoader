using Microsoft.Extensions.Logging;
using System.Text;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Resources.TextAsset;

internal interface ITextAssetPatcher
{
    string[] SubPaths { get; }
    UnityEngine.TextAsset PatchTextAsset(string path, UnityEngine.TextAsset original);
}

internal sealed class TextAssetPatcher<T> : ITextAssetPatcher
    where T : ILeaf
{
    private readonly ILeavesRegistry<T> _registry;
    private readonly ILogger<TextAssetPatcher<T>> _logger;
    private readonly ITextAssetParser<T> _parser;

    public TextAssetPatcher(
        string[] subPaths,
        ILogger<TextAssetPatcher<T>> logger,
        ILeavesRegistry<T> registry,
        ITextAssetParser<T> parser)
    {
        SubPaths = subPaths;
        _logger = logger;
        _parser = parser;
        _registry = registry;
    }

    public string[] SubPaths { get; }

    public UnityEngine.TextAsset PatchTextAsset(string path, UnityEngine.TextAsset original)
    {
        bool registryHasData = _registry.LeavesByNamedIds.Count > 0;
        if (!registryHasData)
            return original;

        IEnumerable<string> newLines = _registry.LeavesByNamedIds.Values
            .OrderBy(i => i.GameId)
            .Select(customLine => _parser.GetTextAssetSerializedString(path, customLine));

        // Some game data relies on having a trailing LF for the parsing to work correctly
        StringBuilder sb = new(string.Join("\n", newLines));
        if (original.text.EndsWith("\n"))
            sb.Append('\n');

        string text = sb.ToString();
        _logger.LogTrace("Patching {path}:\n{text}", path, text);
        return new UnityEngine.TextAsset(text);
    }
}