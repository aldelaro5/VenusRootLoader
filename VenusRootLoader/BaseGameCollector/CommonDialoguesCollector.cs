using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class CommonDialoguesCollector : IBaseGameCollector
{
    private static readonly Dictionary<int, string[]> CommonDialoguesLanguageData =
        RootCollector.ReadLocalizedTestAssetLines(TextAssetPaths.DataLocalizedCommonDialoguesPathSuffix);

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
    }

    public void CollectBaseGameData(string baseGameId)
    {
        int commonDialoguesAmount = CommonDialoguesLanguageData.Values.First().Length;
        for (int i = 0; i < commonDialoguesAmount; i++)
        {
            // Common dialogues do have 0 indexed sequental game ids, but we actually want to use their dialogue ids
            // encoded form for convenience. That form starts at -1 and goes in descending order which is why we need to
            // encode the game id for registration.
            CommonDialogueLeaf commonDialogueLeaf =
                _commonDialoguesRegistry.RegisterExisting(-i - 1, i.ToString(), baseGameId);
            for (int j = 0; j < RootCollector.LanguageDisplayNames.Length; j++)
            {
                _commonDialogueLanguageDataSerializer.FromTextAssetSerializedString(
                    TextAssetPaths.DataLocalizedCommonDialoguesPathSuffix,
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