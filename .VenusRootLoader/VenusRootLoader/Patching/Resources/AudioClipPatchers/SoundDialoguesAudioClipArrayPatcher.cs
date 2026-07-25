using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Resources.AudioClipPatchers;

/// <summary>
/// An <see cref="IAudioClipArrayPatcher"/> that handles patching dialogue bleeps from the game.
/// </summary>
internal sealed class SoundDialoguesAudioClipArrayPatcher : IAudioClipArrayPatcher
{
    private readonly ILeavesRegistry<DialogueBleepLeaf> _dialogueBleepsRegistry;

    public SoundDialoguesAudioClipArrayPatcher(
        string[] subPaths,
        ILeavesRegistry<DialogueBleepLeaf> dialogueBleepsRegistry)
    {
        SubPaths = subPaths;
        _dialogueBleepsRegistry = dialogueBleepsRegistry;
    }

    public string[] SubPaths { get; }

    public AudioClip[] PatchAudioClipArray(string path, AudioClip[] original)
    {
        List<AudioClip> audioClips = new();
        int amountSkipped = 0;
        foreach (AudioClip originalAudioClip in original)
        {
            if (!char.IsDigit(originalAudioClip.name[^1]))
            {
                audioClips.Add(originalAudioClip);
                amountSkipped++;
                continue;
            }

            int gameId = int.Parse(originalAudioClip.name.Replace("Dialogue", string.Empty));
            AudioClip bleepSound = _dialogueBleepsRegistry.LeavesByGameIds[gameId].BleepSound;
            // This is important because the game may use the name to discover what bleep the AudioClip is playing.
            bleepSound.name = $"Dialogue{gameId}";
            audioClips.Add(bleepSound);
        }

        for (int i = original.Length - amountSkipped; i < _dialogueBleepsRegistry.LeavesByGameIds.Count; i++)
        {
            AudioClip bleepSound = _dialogueBleepsRegistry.LeavesByGameIds[i].BleepSound;
            // This is important because the game may use the name to discover what bleep the AudioClip is playing.
            bleepSound.name = $"Dialogue{i}";
            audioClips.Add(bleepSound);
        }

        return audioClips.ToArray();
    }
}