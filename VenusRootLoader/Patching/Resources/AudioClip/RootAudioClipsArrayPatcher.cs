namespace VenusRootLoader.Patching.Resources.AudioClip;

internal sealed class RootAudioClipsArrayPatcher : IResourcesArrayTypePatcher<UnityEngine.AudioClip>
{
    private const string AudioClipsPrefix = "Audio/";

    private readonly Dictionary<string, IAudioClipArrayPatcher> _audioClipArrayPatchers =
        new(StringComparer.OrdinalIgnoreCase);

    public RootAudioClipsArrayPatcher(IEnumerable<IAudioClipArrayPatcher> audioClipArrayPatchers)
    {
        foreach (IAudioClipArrayPatcher audioClipArrayPatcher in audioClipArrayPatchers)
        {
            foreach (string subPath in audioClipArrayPatcher.SubPaths)
                _audioClipArrayPatchers.Add(subPath, audioClipArrayPatcher);
        }
    }

    public UnityEngine.AudioClip[] PatchResources(string path, UnityEngine.AudioClip[] original)
    {
        if (!path.StartsWith(AudioClipsPrefix, StringComparison.OrdinalIgnoreCase))
            return original;

        string subpath = path[AudioClipsPrefix.Length..];
        return _audioClipArrayPatchers.TryGetValue(subpath, out IAudioClipArrayPatcher audioClipArrayPatcher)
            ? audioClipArrayPatcher.PatchAudioClipArray(subpath, original)
            : original;
    }
}