using Microsoft.Extensions.Logging;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Registry;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class MenuTextsCollector : IBaseGameCollector
{
    private static readonly Dictionary<int, string[]> MenuTextsLanguageData = new();

    private readonly ILogger<MenuTextsCollector> _logger;
    private readonly ILeavesRegistry<MenuTextLeaf> _menuTextsRegistry;
    private readonly ILocalizedTextAssetParser<MenuTextLeaf> _menuTextLanguageDataSerializer;

    public MenuTextsCollector(
        ILogger<MenuTextsCollector> logger,
        ILocalizedTextAssetParser<MenuTextLeaf> menuTextLanguageDataSerializer,
        ILeavesRegistry<MenuTextLeaf> menuTextsRegistry)
    {
        _logger = logger;
        _menuTextLanguageDataSerializer = menuTextLanguageDataSerializer;
        _menuTextsRegistry = menuTextsRegistry;

        for (int i = 0; i < RootCollector.LanguageDisplayNames.Length; i++)
        {
            string[] menuTexts = Resources.Load<TextAsset>($"Data/Dialogues{i}/MenuText").text
                .Trim(Utility.StringUtils.NewlineSplitDelimiter)
                .Split(Utility.StringUtils.NewlineSplitDelimiter);
            MenuTextsLanguageData.Add(i, menuTexts);
        }
    }

    public void CollectBaseGameData(string baseGameId)
    {
        int menuTextsAmount = MenuTextsLanguageData.Values.First().Length;
        for (int i = 0; i < menuTextsAmount; i++)
        {
            MenuTextLeaf menuTextLeaf = _menuTextsRegistry.RegisterExisting(i, i.ToString(), baseGameId);
            for (int j = 0; j < RootCollector.LanguageDisplayNames.Length; j++)
            {
                _menuTextLanguageDataSerializer.FromTextAssetSerializedString(
                    "MenuText",
                    j,
                    MenuTextsLanguageData[j][i],
                    menuTextLeaf);
            }
        }

        _logger.LogInformation(
            "Collected and registered {MenuTextsAmount} base game MenuText",
            menuTextsAmount);
    }
}