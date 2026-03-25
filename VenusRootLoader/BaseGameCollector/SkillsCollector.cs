using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class SkillsCollector : IBaseGameCollector
{
    private static readonly string[] SkillsData = RootCollector.ReadTextAssetLines(TextAssetPaths.DataSkillsPath);

    private static readonly Dictionary<int, string[]> SkillsLanguageData =
        RootCollector.ReadLocalizedTestAssetLines(TextAssetPaths.DataLocalizedSkillsPathSuffix);

    private readonly string[] _skillNamedIds = Enum.GetNames(typeof(MainManager.Skills)).ToArray();

    private readonly ILogger<SkillsCollector> _logger;
    private readonly ITextAssetParser<SkillLeaf> _skillTextAssetParser;
    private readonly ILocalizedTextAssetParser<SkillLeaf> _skillLocalizedTextAssetParser;
    private readonly ILeavesRegistry<SkillLeaf> _skillsRegistry;

    public SkillsCollector(
        ILogger<SkillsCollector> logger,
        ITextAssetParser<SkillLeaf> skillTextAssetParser,
        ILocalizedTextAssetParser<SkillLeaf> skillLocalizedTextAssetParser,
        ILeavesRegistry<SkillLeaf> skillsRegistry)
    {
        _logger = logger;
        _skillTextAssetParser = skillTextAssetParser;
        _skillLocalizedTextAssetParser = skillLocalizedTextAssetParser;
        _skillsRegistry = skillsRegistry;
    }

    public void CollectBaseGameData(string baseGameId)
    {
        int skillsAmount = _skillNamedIds.Length;
        for (int i = 0; i < skillsAmount; i++)
        {
            SkillLeaf skillLeaf = _skillsRegistry.RegisterExisting(i, _skillNamedIds[i], baseGameId);
            _skillTextAssetParser.FromTextAssetSerializedString(
                TextAssetPaths.DataSkillsPath,
                SkillsData[i],
                skillLeaf);
            for (int j = 0; j < RootCollector.LanguageDisplayNames.Length; j++)
            {
                skillLeaf.LocalizedData[j] = new();
                _skillLocalizedTextAssetParser.FromTextAssetSerializedString(
                    TextAssetPaths.DataLocalizedSkillsPathSuffix,
                    j,
                    SkillsLanguageData[j][i],
                    skillLeaf);
            }
        }

        _logger.LogInformation("Collected and registered {SkillsAmount} base game skills", skillsAmount);
    }
}