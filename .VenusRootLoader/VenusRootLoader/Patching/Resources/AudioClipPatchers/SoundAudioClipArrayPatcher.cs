using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Resources.AudioClipPatchers;

/// <summary>
/// An <see cref="IAudioClipArrayPatcher"/> that handles patching sounds from the game using the <see cref="SoundLeaf"/> registry.
/// </summary>
internal sealed class SoundAudioClipArrayPatcher : IAudioClipArrayPatcher
{
    private readonly ILeavesRegistry<SoundLeaf> _soundsRegistry;

    public SoundAudioClipArrayPatcher(
        string[] subPaths,
        ILeavesRegistry<SoundLeaf> soundsRegistry)
    {
        SubPaths = subPaths;
        _soundsRegistry = soundsRegistry;
    }

    public string[] SubPaths { get; }

    public AudioClip[] PatchAudioClipArray(string path, AudioClip[] original)
    {
        List<AudioClip> audioClips = new();
        HashSet<string> processedEffectiveIds = [];
        // The bleeps are in a subdirectory of the sounds so we need to exclude them.
        List<AudioClip> sanitizedOriginals = original
            .Where(x => !x.name.StartsWith("Dialogue"))
            .ToList();
        foreach (AudioClip originalAudioClip in sanitizedOriginals)
        {
            string effectiveId = originalAudioClip.name;
            // BeepLoop is a sound that exists twice with one version being unused. We only want to load it once from our
            // registry and throw out the unused one.
            if (!processedEffectiveIds.Add(effectiveId))
                continue;
            AudioClip sound = _soundsRegistry.GetByEffectiveId(effectiveId).Sound.LoadAsset();
            // This is important because the game may use the name to discover what sound the AudioClip is playing.
            sound.name = effectiveId;
            audioClips.Add(sound);
        }

        for (int i = audioClips.Count; i < _soundsRegistry.Count; i++)
        {
            SoundLeaf soundLeaf = _soundsRegistry.GetByGameId(i);
            AudioClip sound = soundLeaf.Sound.LoadAsset();
            // This is important because the game may use the name to discover what sound the AudioClip is playing.
            sound.name = soundLeaf.EffectiveId;
            audioClips.Add(sound);
        }

        return audioClips.ToArray();
    }
}