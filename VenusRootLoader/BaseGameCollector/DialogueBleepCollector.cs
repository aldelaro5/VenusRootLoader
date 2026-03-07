using Microsoft.Extensions.Logging;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class DialogueBleepCollector : IBaseGameCollector
{
    private readonly AudioClip[] _dialogueBleeps = Resources.LoadAll<AudioClip>("Audio/Sounds/Dialogue");

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