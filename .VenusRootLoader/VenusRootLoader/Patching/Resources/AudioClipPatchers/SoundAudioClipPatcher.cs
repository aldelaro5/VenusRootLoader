using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.AudioClipPatchers;

/// <summary>
/// An <see cref="IAudioClipPatcher"/> that handles patching sounds from the game using the <see cref="SoundLeaf"/> registry.
/// </summary>
internal sealed class SoundAudioClipPatcher : IAudioClipPatcher
{
    private readonly ILeavesRegistry<SoundLeaf> _soundsRegistry;

    public SoundAudioClipPatcher(
        string[] subPaths,
        ILeavesRegistry<SoundLeaf> soundsRegistry)
    {
        SubPaths = subPaths;
        _soundsRegistry = soundsRegistry;
    }

    public string[] SubPaths { get; }

    public AudioClip PatchAudioClip(string path, AudioClip original)
    {
        string effectiveId = path.Replace($"{ResourcesPaths.AudioSoundsDirectory}/", string.Empty);
        AudioClip sound = _soundsRegistry.GetByEffectiveId(effectiveId).Sound.LoadAsset();
        // This is important because the game may use the name to discover what sound the AudioClip is playing.
        sound.name = effectiveId;
        return sound;
    }
}