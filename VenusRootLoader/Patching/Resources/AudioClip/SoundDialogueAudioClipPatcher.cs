using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Resources.AudioClip;

internal sealed class SoundDialoguesAudioClipPatcher : IAudioClipPatcher
{
    private readonly ILeavesRegistry<DialogueBleepLeaf> _dialogueBleepsRegistry;

    public SoundDialoguesAudioClipPatcher(
        string[] subPaths,
        ILeavesRegistry<DialogueBleepLeaf> dialogueBleepsRegistry)
    {
        SubPaths = subPaths;
        _dialogueBleepsRegistry = dialogueBleepsRegistry;
    }

    public string[] SubPaths { get; }

    public UnityEngine.AudioClip PatchAudioClip(string path, UnityEngine.AudioClip original)
    {
        if (!char.IsDigit(path[^1]))
            return original;

        int gameId = int.Parse(path.Replace("Sounds/Dialogue/Dialogue", string.Empty));
        UnityEngine.AudioClip bleepSound = _dialogueBleepsRegistry.LeavesByGameIds[gameId].BleepSound;
        bleepSound.name = $"Dialogue{gameId}";
        return bleepSound;
    }
}