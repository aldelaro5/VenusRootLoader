using Microsoft.Extensions.Logging;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class SkillsCollector : IBaseGameCollector
{
    private static readonly string[] SkillsData = Resources
        .Load<TextAsset>($"{TextAssetPaths.RootDataPathPrefix}{TextAssetPaths.DataSkillsPath}").text
        .Trim('\n')
        .Split(['\n'], StringSplitOptions.RemoveEmptyEntries);

    private static readonly Dictionary<int, string[]> SkillsLanguageData = new();

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

        for (int i = 0; i < RootCollector.LanguageDisplayNames.Length; i++)
        {
            string[] skillLanguageData = Resources.Load<TextAsset>(
                    $"{TextAssetPaths.DataSlashDialogues}{i}/{TextAssetPaths.DataLocalizedSkillsPathSuffix}").text
                .Trim(StringUtils.NewlineSplitDelimiter)
                .Split(StringUtils.NewlineSplitDelimiter, StringSplitOptions.RemoveEmptyEntries);
            SkillsLanguageData.Add(i, skillLanguageData);
        }
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