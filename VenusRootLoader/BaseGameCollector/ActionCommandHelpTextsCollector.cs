using Microsoft.Extensions.Logging;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

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
            string[] actionCommandHelpText = Resources
                .Load<TextAsset>(
                    $"{TextAssetPaths.DataSlashDialogues}{i}/{TextAssetPaths.DataLocalizedActionCommandHelpTextsPathSuffix}")
                .text
                .Trim(StringUtils.NewlineSplitDelimiter)
                .Split(StringUtils.NewlineSplitDelimiter, StringSplitOptions.RemoveEmptyEntries);
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
                    TextAssetPaths.DataLocalizedActionCommandHelpTextsPathSuffix,
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