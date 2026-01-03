namespace VenusRootLoader.Patching.Resources.TextAsset;

internal sealed class RootTextAssetPatcher : IResourcesTypePatcher<UnityEngine.TextAsset>
{
    private const string DataPrefix = "Data/";
    private const string LocalizedPathPrefix = "Data/Dialogues";
    private static readonly char[] LocalisedPathSeparator = ['/'];

    private readonly Dictionary<string, ITextAssetPatcher> _textAssetPatchers =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly Dictionary<string, ILocalizedTextAssetPatcher> _localizedTextAssetPatchers =
        new(StringComparer.OrdinalIgnoreCase);

    public RootTextAssetPatcher(
        IEnumerable<ITextAssetPatcher> textAssetPatchers,
        IEnumerable<ILocalizedTextAssetPatcher> localizedTextAssetPatchers)
    {
        foreach (ITextAssetPatcher textAssetPatcher in textAssetPatchers)
        {
            foreach (string subPath in textAssetPatcher.SubPaths)
                _textAssetPatchers.Add(subPath, textAssetPatcher);
        }

        foreach (ILocalizedTextAssetPatcher localizedTextAssetPatcher in localizedTextAssetPatchers)
        {
            foreach (string subPath in localizedTextAssetPatcher.SubPaths)
                _localizedTextAssetPatchers.Add(subPath, localizedTextAssetPatcher);
        }
    }

    public UnityEngine.TextAsset PatchResource(string path, UnityEngine.TextAsset original)
    {
        if (!path.StartsWith(DataPrefix, StringComparison.OrdinalIgnoreCase))
            return original;

        if (path.StartsWith(LocalizedPathPrefix, StringComparison.OrdinalIgnoreCase))
        {
            string[] localizedPathParts = path[LocalizedPathPrefix.Length..].Split(LocalisedPathSeparator);
            int languageId = int.Parse(localizedPathParts[0]);
            string subpathLocalized = localizedPathParts[1];
            return _localizedTextAssetPatchers.TryGetValue(subpathLocalized, out ILocalizedTextAssetPatcher patcher)
                ? patcher.PatchResource(languageId, string.Join("/", localizedPathParts.Skip(1)), original)
                : original;
        }

        string[] pathParts = path[DataPrefix.Length..].Split(LocalisedPathSeparator);
        string subpath = pathParts[0];
        return _textAssetPatchers.TryGetValue(subpath, out ITextAssetPatcher textAssetPatcher)
            ? textAssetPatcher.PatchResource(string.Join("/", pathParts.Skip(1)), original)
            : original;
    }
}