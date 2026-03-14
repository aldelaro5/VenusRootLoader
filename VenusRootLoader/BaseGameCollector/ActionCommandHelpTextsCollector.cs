using Microsoft.Extensions.Logging;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAssetPatchers;
using VenusRootLoader.Registry;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class ActionCommandHelpTextsCollector : IBaseGameCollector
{
    private static readonly Dictionary<int, string[]> ActionCommandHelpTextsLanguageData = new();

    private readonly ILogger<ActionCommandHelpTextsCollector> _logger;
    private readonly ILeavesRegistry<ActionCommandHelpTextLeaf> _actionCommandHelpTextsRegistry;

    private readonly ILocalizedTextAssetParser<ActionCommandHelpTextLeaf>
        _actionCommandHelpTextLocalizedTextAssetParser;

    public ActionCommandHelpTextsCollector(
        ILogger<ActionCommandHelpTextsCollector> logger,
        ILocalizedTextAssetParser<ActionCommandHelpTextLeaf> actionCommandHelpTextLocalizedTextAssetParser,
        ILeavesRegistry<ActionCommandHelpTextLeaf> actionCommandHelpTextsRegistry)
    {
        _logger = logger;
        _actionCommandHelpTextLocalizedTextAssetParser = actionCommandHelpTextLocalizedTextAssetParser;
        _actionCommandHelpTextsRegistry = actionCommandHelpTextsRegistry;

        for (int i = 0; i < RootCollector.LanguageDisplayNames.Length; i++)
        {
            string[] actionCommandHelpText = Resources.Load<TextAsset>($"Data/Dialogues{i}/ActionCommands").text
                .Trim(Utility.StringUtils.NewlineSplitDelimiter)
                .Split(Utility.StringUtils.NewlineSplitDelimiter, StringSplitOptions.RemoveEmptyEntries);
            ActionCommandHelpTextsLanguageData.Add(i, actionCommandHelpText);
        }
    }

    public void CollectBaseGameData(string baseGameId)
    {
        int actionCommandHelpTextsAmount = ActionCommandHelpTextsLanguageData.Values.First().Length;
        for (int i = 0; i < actionCommandHelpTextsAmount; i++)
        {
            ActionCommandHelpTextLeaf actionCommandHelpTextLeaf =
                _actionCommandHelpTextsRegistry.RegisterExisting(i, i.ToString(), baseGameId);
            for (int j = 0; j < RootCollector.LanguageDisplayNames.Length; j++)
            {
                _actionCommandHelpTextLocalizedTextAssetParser.FromTextAssetSerializedString(
                    "ActionCommands",
                    j,
                    ActionCommandHelpTextsLanguageData[j][i],
                    actionCommandHelpTextLeaf);
            }
        }

        _logger.LogInformation(
            "Collected and registered {ActionCommandHelpTextsAmount} base game action command help texts",
            actionCommandHelpTextsAmount);
    }
}