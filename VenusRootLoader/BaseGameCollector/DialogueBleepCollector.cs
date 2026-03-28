using Microsoft.Extensions.Logging;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class DialogueBleepCollector : IBaseGameCollector
{
    private readonly AudioClip[] _dialogueBleeps = Resources.LoadAll<AudioClip>(
        $"{TextAssetPaths.RootAudioPathPrefix}{TextAssetPaths.AudioSoundsDialogueDirectory}");

    private readonly ILogger<DialogueBleepCollector> _logger;
    private readonly ILeavesRegistry<DialogueBleepLeaf> _dialogueBleepsRegistry;

    public DialogueBleepCollector(
        ILogger<DialogueBleepCollector> logger,
        ILeavesRegistry<DialogueBleepLeaf> dialogueBleepsRegistry)
    {
        _logger = logger;
        _dialogueBleepsRegistry = dialogueBleepsRegistry;
    }

    public void CollectBaseGameData(string baseGameId)
    {
        // We need to strip out clips like Dialogue3old which aren't considered bleeps that can be addressed as such.
        // They are effectively unused in base game.
        List<AudioClip> dialogueBleeps = _dialogueBleeps
            .Where(a => char.IsDigit(a.name[^1]))
            .OrderBy(a => int.Parse(a.name.Replace("Dialogue", string.Empty)))
            .ToList();
        for (int i = 0; i < dialogueBleeps.Count; i++)
        {
            DialogueBleepLeaf dialogueBleepLeaf = _dialogueBleepsRegistry.RegisterExisting(i, i.ToString(), baseGameId);
            dialogueBleepLeaf.BleepSound = dialogueBleeps[i];
        }

        _logger.LogInformation(
            "Collected and registered {DialogueBleepsAmount} base game dialogue bleeps",
            dialogueBleeps.Count);
    }
}