using UnityEngine;

namespace VenusRootLoader.Patching.Resources.AudioClipPatchers;

internal sealed class RootAudioClipPatcher : IResourcesTypePatcher<AudioClip>
{
    private const string AudioClipsPrefix = "Audio/";

    private readonly Dictionary<string, IAudioClipPatcher> _audioClipPatchers =
        new(StringComparer.OrdinalIgnoreCase);

    public RootAudioClipPatcher(IEnumerable<IAudioClipPatcher> audioClipPatchers)
    {
        foreach (IAudioClipPatcher audioClipArrayPatcher in audioClipPatchers)
        {
            foreach (string subPath in audioClipArrayPatcher.SubPaths)
                _audioClipPatchers.Add(subPath, audioClipArrayPatcher);
        }
    }

    public AudioClip PatchResource(string path, AudioClip original)
    {
        if (!path.StartsWith(AudioClipsPrefix, StringComparison.OrdinalIgnoreCase))
            return original;

        string audioSubpath = path[AudioClipsPrefix.Length..];
        string subpath = audioSubpath[..audioSubpath.LastIndexOf('/')];
        return _audioClipPatchers.TryGetValue(subpath, out IAudioClipPatcher audioClipPatcher)
            ? audioClipPatcher.PatchAudioClip(audioSubpath, original)
            : original;
    }
}