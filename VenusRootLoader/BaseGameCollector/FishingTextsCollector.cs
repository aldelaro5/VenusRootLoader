using Microsoft.Extensions.Logging;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Registry;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class FishingTextsCollector : IBaseGameCollector
{
    private static readonly Dictionary<int, string[]> FishingTextsLanguageData = new();

    private readonly ILogger<FishingTextsCollector> _logger;
    private readonly ILeavesRegistry<FishingTextLeaf> _fishingTextsRegistry;
    private readonly ILocalizedTextAssetParser<FishingTextLeaf> _fishingTextLocalizedTextAssetParser;

    public FishingTextsCollector(
        ILogger<FishingTextsCollector> logger,
        ILocalizedTextAssetParser<FishingTextLeaf> fishingTextLocalizedTextAssetParser,
        ILeavesRegistry<FishingTextLeaf> fishingTextsRegistry)
    {
        _logger = logger;
        _fishingTextLocalizedTextAssetParser = fishingTextLocalizedTextAssetParser;
        _fishingTextsRegistry = fishingTextsRegistry;

        for (int i = 0; i < RootCollector.LanguageDisplayNames.Length; i++)
        {
            string[] fishingText = Resources.Load<TextAsset>($"Data/Dialogues{i}/Fishing").text
                .Trim(Utility.StringUtils.NewlineSplitDelimiter)
                .Split(Utility.StringUtils.NewlineSplitDelimiter, StringSplitOptions.RemoveEmptyEntries);
            FishingTextsLanguageData.Add(i, fishingText);
        }
    }

    public void CollectBaseGameData(string baseGameId)
    {
        int fishingTextsAmount = FishingTextsLanguageData.Values.First().Length;
        for (int i = 0; i < fishingTextsAmount; i++)
        {
            FishingTextLeaf fishingTextLeaf = _fishingTextsRegistry.RegisterExisting(i, i.ToString(), baseGameId);
            for (int j = 0; j < RootCollector.LanguageDisplayNames.Length; j++)
            {
                _fishingTextLocalizedTextAssetParser.FromTextAssetSerializedString(
                    "Fishing",
                    j,
                    FishingTextsLanguageData[j][i],
                    fishingTextLeaf);
            }
        }

        _logger.LogInformation(
            "Collected and registered {FishingTextsAmount} base game fishing texts",
            fishingTextsAmount);
    }
}