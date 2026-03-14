using UnityEngine;

namespace VenusRootLoader.Patching.Resources.AudioClipPatchers;

internal sealed class RootAudioClipsArrayPatcher : IResourcesArrayTypePatcher<AudioClip>
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

    public AudioClip[] PatchResources(string path, AudioClip[] original)
    {
        if (!path.StartsWith(AudioClipsPrefix, StringComparison.OrdinalIgnoreCase))
            return original;

        string subpath = path[AudioClipsPrefix.Length..];
        return _audioClipArrayPatchers.TryGetValue(subpath, out IAudioClipArrayPatcher audioClipArrayPatcher)
            ? audioClipArrayPatcher.PatchAudioClipArray(subpath, original)
            : original;
    }
}