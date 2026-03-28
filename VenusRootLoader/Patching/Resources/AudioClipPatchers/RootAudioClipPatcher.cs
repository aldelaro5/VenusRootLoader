using UnityEngine;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.AudioClipPatchers;

/// <summary>
/// An <see cref="IResourcesTypePatcher{TObject}"/> that patches <see cref="AudioClip"/> assets.
/// It mainly dispatches the actual patching to any concerned <see cref="IAudioClipPatcher"/> depending on the resource path.
/// </summary>
internal sealed class RootAudioClipPatcher : IResourcesTypePatcher<AudioClip>
{
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
        if (!path.StartsWith(TextAssetPaths.RootAudioPathPrefix, StringComparison.OrdinalIgnoreCase))
            return original;

        string audioSubpath = path[TextAssetPaths.RootAudioPathPrefix.Length..];
        string subpath = audioSubpath[..audioSubpath.LastIndexOf('/')];
        return _audioClipPatchers.TryGetValue(subpath, out IAudioClipPatcher audioClipPatcher)
            ? audioClipPatcher.PatchAudioClip(audioSubpath, original)
            : original;
    }
}