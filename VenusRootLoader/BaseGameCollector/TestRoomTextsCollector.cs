using Microsoft.Extensions.Logging;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.BaseGameCollector;

internal sealed class TestRoomTextsCollector : IBaseGameCollector
{
    private static readonly string[] TestRoomTextsData = Resources
        .Load<TextAsset>($"{TextAssetPaths.RootDataPathPrefix}{TextAssetPaths.DataTestRoomMapDialoguesPath}").text
        .Trim('\n')
        .Split(['\n'], StringSplitOptions.RemoveEmptyEntries);

    private readonly ILogger<TestRoomTextsCollector> _logger;
    private readonly ILeavesRegistry<TestRoomTextLeaf> _testRoomTextsRegistry;
    private readonly ITextAssetParser<TestRoomTextLeaf> _testRoomTextTextAssetParser;

    public TestRoomTextsCollector(
        ILogger<TestRoomTextsCollector> logger,
        ILeavesRegistry<TestRoomTextLeaf> testRoomTextsRegistry,
        ITextAssetParser<TestRoomTextLeaf> testRoomTextTextAssetParser)
    {
        _logger = logger;
        _testRoomTextsRegistry = testRoomTextsRegistry;
        _testRoomTextTextAssetParser = testRoomTextTextAssetParser;
    }

    public void CollectBaseGameData(string baseGameId)
    {
        int testRoomTextsAmount = TestRoomTextsData.Length;
        for (int i = 0; i < testRoomTextsAmount; i++)
        {
            TestRoomTextLeaf testRoomTextLeaf = _testRoomTextsRegistry.RegisterExisting(i, i.ToString(), baseGameId);
            _testRoomTextTextAssetParser.FromTextAssetSerializedString(
                TextAssetPaths.DataTestRoomMapDialoguesPath,
                TestRoomTextsData[i],
                testRoomTextLeaf);
        }

        _logger.LogInformation(
            "Collected and registered {TestRoomTextsAmount} base game test room texts",
            testRoomTextsAmount);
    }
}