using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class SkillActionCommandHelpTextsCollector : IBaseGameCollector
{
    private readonly Dictionary<int, string[]> _actionCommandHelpTextsLanguageData =
        RootCollector.ReadLocalizedTestAssetLines(ResourcesPaths.DataLocalizedActionCommandHelpTextsPathSuffix);

    private readonly ILogger<SkillActionCommandHelpTextsCollector> _logger;
    private readonly ILeavesRegistry<SkillActionCommandHelpTextLeaf> _actionCommandHelpTextsRegistry;

    private readonly ILocalizedTextAssetParser<SkillActionCommandHelpTextLeaf>
        _actionCommandHelpTextLocalizedTextAssetParser;

    public SkillActionCommandHelpTextsCollector(
        ILogger<SkillActionCommandHelpTextsCollector> logger,
        ILocalizedTextAssetParser<SkillActionCommandHelpTextLeaf> actionCommandHelpTextLocalizedTextAssetParser,
        ILeavesRegistry<SkillActionCommandHelpTextLeaf> actionCommandHelpTextsRegistry)
    {
        _logger = logger;
        _actionCommandHelpTextLocalizedTextAssetParser = actionCommandHelpTextLocalizedTextAssetParser;
        _actionCommandHelpTextsRegistry = actionCommandHelpTextsRegistry;
    }

    public void CollectBaseGameData()
    {
        int actionCommandHelpTextsAmount = _actionCommandHelpTextsLanguageData.Values.First().Length;
        for (int i = 0; i < actionCommandHelpTextsAmount; i++)
        {
            SkillActionCommandHelpTextLeaf skillActionCommandHelpTextLeaf =
                _actionCommandHelpTextsRegistry.RegisterExisting(i, i.ToString());
            for (int j = 0; j < RootCollector.LanguageDisplayNames.Length; j++)
            {
                _actionCommandHelpTextLocalizedTextAssetParser.FromTextAssetSerializedString(
                    ResourcesPaths.DataLocalizedActionCommandHelpTextsPathSuffix,
                    j,
                    _actionCommandHelpTextsLanguageData[j][i],
                    skillActionCommandHelpTextLeaf);
            }
        }

        RootCollector.LogCollectedAmount(_logger, _actionCommandHelpTextsRegistry, actionCommandHelpTextsAmount);
    }
}