using Microsoft.Extensions.Logging;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAssetPatchers;
using VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;
using VenusRootLoader.Registry;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class CommonDialoguesCollector : IBaseGameCollector
{
    private static readonly Dictionary<int, string[]> CommonDialoguesLanguageData = new();

    private readonly ILogger<CommonDialoguesCollector> _logger;
    private readonly ILeavesRegistry<CommonDialogueLeaf> _commonDialoguesRegistry;
    private readonly ILocalizedTextAssetParser<CommonDialogueLeaf> _commonDialogueLanguageDataSerializer;

    public CommonDialoguesCollector(
        ILogger<CommonDialoguesCollector> logger,
        ILocalizedTextAssetParser<CommonDialogueLeaf> commonDialogueLanguageDataSerializer,
        ILeavesRegistry<CommonDialogueLeaf> commonDialoguesRegistry)
    {
        _logger = logger;
        _commonDialogueLanguageDataSerializer = commonDialogueLanguageDataSerializer;
        _commonDialoguesRegistry = commonDialoguesRegistry;

        for (int i = 0; i < RootCollector.LanguageDisplayNames.Length; i++)
        {
            string[] commonDialogues = Resources.Load<TextAsset>($"Data/Dialogues{i}/CommonDialogue").text
                .Trim(Utility.StringUtils.NewlineSplitDelimiter)
                .Split(Utility.StringUtils.NewlineSplitDelimiter, StringSplitOptions.RemoveEmptyEntries);
            CommonDialoguesLanguageData.Add(i, commonDialogues);
        }
    }

    public void CollectBaseGameData(string baseGameId)
    {
        int commonDialoguesAmount = CommonDialoguesLanguageData.Values.First().Length;
        for (int i = 0; i < commonDialoguesAmount; i++)
        {
            CommonDialogueLeaf commonDialogueLeaf =
                _commonDialoguesRegistry.RegisterExisting(-i - 1, i.ToString(), baseGameId);
            for (int j = 0; j < RootCollector.LanguageDisplayNames.Length; j++)
            {
                _commonDialogueLanguageDataSerializer.FromTextAssetSerializedString(
                    "CommonDialogue",
                    j,
                    CommonDialoguesLanguageData[j][i],
                    commonDialogueLeaf);
            }
        }

        _logger.LogInformation(
            "Collected and registered {CommonDialoguesAmount} base game CommonDialogue",
            commonDialoguesAmount);
    }
}