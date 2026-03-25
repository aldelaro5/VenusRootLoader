using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class MenuTextsCollector : IBaseGameCollector
{
    private static readonly Dictionary<int, string[]> MenuTextsLanguageData =
        RootCollector.ReadLocalizedTestAssetLines(TextAssetPaths.DataLocalizedMenuTextsPathSuffix);

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
                    TextAssetPaths.DataLocalizedMenuTextsPathSuffix,
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