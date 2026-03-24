using UnityEngine;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers;

internal sealed class RootTextAssetPatcher : IResourcesTypePatcher<TextAsset>
{
    private readonly Dictionary<string, ITextAssetPatcher> _textAssetPatchers =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly Dictionary<string, IOrderingTextAssetPatcher> _orderingTextAssetPatchers =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly Dictionary<string, ILocalizedTextAssetPatcher> _localizedTextAssetPatchers =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly IMapEntityTextAssetPatcher _mapEntityTextAssetPatcher;
    private readonly IMapDialoguesTextAssetPatcher _mapDialoguesTextAssetPatcher;

    public RootTextAssetPatcher(
        IEnumerable<ITextAssetPatcher> textAssetPatchers,
        IEnumerable<IOrderingTextAssetPatcher> orderingTextAssetPatchers,
        IEnumerable<ILocalizedTextAssetPatcher> localizedTextAssetPatchers,
        IMapEntityTextAssetPatcher mapEntityTextAssetPatcher,
        IMapDialoguesTextAssetPatcher mapDialoguesTextAssetPatcher)
    {
        _mapEntityTextAssetPatcher = mapEntityTextAssetPatcher;
        _mapDialoguesTextAssetPatcher = mapDialoguesTextAssetPatcher;

        foreach (ITextAssetPatcher textAssetPatcher in textAssetPatchers)
        {
            foreach (string subPath in textAssetPatcher.SubPaths)
                _textAssetPatchers.Add(subPath, textAssetPatcher);
        }

        foreach (IOrderingTextAssetPatcher orderingTextAssetPatcher in orderingTextAssetPatchers)
        {
            _orderingTextAssetPatchers.Add(orderingTextAssetPatcher.SubPath, orderingTextAssetPatcher);
        }

        foreach (ILocalizedTextAssetPatcher localizedTextAssetPatcher in localizedTextAssetPatchers)
        {
            foreach (string subPath in localizedTextAssetPatcher.SubPaths)
                _localizedTextAssetPatchers.Add(subPath, localizedTextAssetPatcher);
        }
    }

    public TextAsset PatchResource(string path, TextAsset original)
    {
        if (!path.StartsWith(TextAssetPaths.RootDataPathPrefix, StringComparison.OrdinalIgnoreCase))
            return original;

        string textAssetSubpath = path[TextAssetPaths.RootDataPathPrefix.Length..];
        if (textAssetSubpath.StartsWith(TextAssetPaths.DataMapEntitiesDirectory, StringComparison.OrdinalIgnoreCase))
            return _mapEntityTextAssetPatcher.PatchMapEntityTextAsset(textAssetSubpath, original);

        if (textAssetSubpath.StartsWith(
                TextAssetPaths.DataLocalizedDialoguesDirectoryPrefix,
                StringComparison.OrdinalIgnoreCase))
        {
            return PatchLocalizedTextAsset(textAssetSubpath, original);
        }

        if (_textAssetPatchers.TryGetValue(textAssetSubpath, out ITextAssetPatcher specificPrefabPatcher))
            return specificPrefabPatcher.PatchTextAsset(textAssetSubpath, original);
        if (_orderingTextAssetPatchers.TryGetValue(textAssetSubpath, out IOrderingTextAssetPatcher orderingPatcher))
            return orderingPatcher.PatchTextAsset(textAssetSubpath, original);

        int lastIndexSlash = textAssetSubpath.LastIndexOf('/');
        if (lastIndexSlash == -1)
            return original;

        string subpath = textAssetSubpath[..lastIndexSlash];
        return _textAssetPatchers.TryGetValue(subpath, out ITextAssetPatcher textAssetPatcher)
            ? textAssetPatcher.PatchTextAsset(textAssetSubpath, original)
            : original;
    }

    private TextAsset PatchLocalizedTextAsset(string textAssetSubpath, TextAsset original)
    {
        string localizedSubPath = textAssetSubpath[TextAssetPaths.DataLocalizedDialoguesDirectoryPrefix.Length..];
        int firstSlash = localizedSubPath.IndexOf('/');
        int languageId = int.Parse(localizedSubPath[..firstSlash]);
        string subPath = localizedSubPath[(firstSlash + 1)..];

        if (subPath.StartsWith(TextAssetPaths.DataDialoguesLocalizedMapsDirectory, StringComparison.OrdinalIgnoreCase))
            return _mapDialoguesTextAssetPatcher.PatchMapDialoguesTextAsset(languageId, textAssetSubpath, original);

        return _localizedTextAssetPatchers.TryGetValue(subPath, out ILocalizedTextAssetPatcher patcher)
            ? patcher.PatchLocalisedTextAsset(languageId, textAssetSubpath, original)
            : original;
    }
}