using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Logic.LeavesSupport;

/// <summary>
/// This patcher allows the <see cref="SoundLeaf"/> and <see cref="MusicLeaf"/> registries to function with special
/// sources of AudioClips where they are referenced from other Unity assets instead of being resources loaded.
/// <p>
/// It patches the following:
/// <list type="bullet">
/// <item><see cref="SoundControl.Start"/>: Overrides the source's AudioClip to reflect the <see cref="ActionCommandLeaf"/> registry.</item>
/// <item><see cref="MusicSpinner.Start"/>: Overrides the AudioClip to reflect the <see cref="ActionCommandLeaf"/> registry.</item>
/// <item><see cref="FishingMain.Start"/>: Overrides the fish line's source's AudioClip and the music preloads to reflect
/// the <see cref="SoundLeaf"/> and <see cref="MusicLeaf"/> registries.</item>
/// </list>
/// </p>
/// </summary>
internal sealed class AdditionalAudioClipTopLevelPatcher : ITopLevelPatcher
{
    private static AdditionalAudioClipTopLevelPatcher _instance = null!;

    private readonly ILeavesRegistry<SoundLeaf> _soundsRegistry;
    private readonly ILeavesRegistry<MusicLeaf> _musicsRegistry;

    public AdditionalAudioClipTopLevelPatcher(
        ILeavesRegistry<SoundLeaf> soundsRegistry,
        ILeavesRegistry<MusicLeaf> musicsRegistry)
    {
        _instance = this;
        _soundsRegistry = soundsRegistry;
        _musicsRegistry = musicsRegistry;
    }

    public void Patch()
    {
        Md.SoundControl.Start.Postfix(PatchSoundControl);
        Md.MusicSpinner.Start.Postfix(PatchMusicSpinner);
        // Has to be a prefix because Start will start using the clips assigned early.
        Md.FishingMain.Start.Prefix(PatchFishingMain);
    }

    private static void PatchSoundControl(SoundControl self)
    {
        AudioClip originalClip = self.source.clip;
        // This can happen under normal gameplay.
        if (originalClip == null)
            return;
        if (!_instance._soundsRegistry.TryGetByEffectiveId(originalClip.name, out SoundLeaf? soundLeaf))
            return;

        bool wasPlaying = self.source.isPlaying;
        self.source.clip = soundLeaf.Sound.LoadAsset();
        if (wasPlaying)
            self.source.Play();
    }

    private static void PatchMusicSpinner(MusicSpinner self)
    {
        // Normally shouldn't happen, but just to be sure, we might as well null check it.
        if (self.soundclip == null)
            return;
        if (!_instance._soundsRegistry.TryGetByEffectiveId(self.soundclip.name, out SoundLeaf? soundLeaf))
            return;
        self.soundclip = soundLeaf.Sound.LoadAsset();
    }

    private static void PatchFishingMain(FishingMain self)
    {
        self.lineSound = self.GetComponent<AudioSource>();
        AudioClip originalClip = self.lineSound.clip;
        if (!_instance._soundsRegistry.TryGetByEffectiveId(originalClip.name, out SoundLeaf? soundLeaf))
            return;
        self.lineSound.clip = soundLeaf.Sound.LoadAsset();

        for (int i = 0; i < self.musicPreload.Length; i++)
        {
            AudioClip originalMusic = self.musicPreload[i];
            if (!_instance._musicsRegistry.TryGetByEffectiveId(originalMusic.name, out MusicLeaf? musicLeaf))
                return;
            self.musicPreload[i] = musicLeaf.Music.LoadAsset();
        }
    }
}