using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class ActionCommandHelpTextsCollector : IBaseGameCollector
{
    private readonly Dictionary<int, string[]> _actionCommandHelpTextsLanguageData =
        RootCollector.ReadLocalizedTestAssetLines(TextAssetPaths.DataLocalizedActionCommandHelpTextsPathSuffix);

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
    }

    public void CollectBaseGameData(string baseGameId)
    {
        int actionCommandHelpTextsAmount = _actionCommandHelpTextsLanguageData.Values.First().Length;
        for (int i = 0; i < actionCommandHelpTextsAmount; i++)
        {
            ActionCommandHelpTextLeaf actionCommandHelpTextLeaf =
                _actionCommandHelpTextsRegistry.RegisterExisting(i, i.ToString(), baseGameId);
            for (int j = 0; j < RootCollector.LanguageDisplayNames.Length; j++)
            {
                _actionCommandHelpTextLocalizedTextAssetParser.FromTextAssetSerializedString(
                    TextAssetPaths.DataLocalizedActionCommandHelpTextsPathSuffix,
                    j,
                    _actionCommandHelpTextsLanguageData[j][i],
                    actionCommandHelpTextLeaf);
            }
        }

        _logger.LogInformation(
            "Collected and registered {ActionCommandHelpTextsAmount} base game action command help texts",
            actionCommandHelpTextsAmount);
    }
}