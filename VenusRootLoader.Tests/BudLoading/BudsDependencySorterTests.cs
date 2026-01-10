using AsmResolver.PE.DotNet.Metadata.Tables;
using AwesomeAssertions;
using Microsoft.Extensions.Logging.Testing;
using VenusRootLoader.Api;
using VenusRootLoader.BudLoading;

namespace VenusRootLoader.Tests.BudLoading;

public sealed class BudsDependencySorterTests
{
    private readonly FakeLogger<BudsDependencySorter> _logger = new();

    private readonly BudsDependencySorter _sut;

    public BudsDependencySorterTests()
    {
        _sut = new BudsDependencySorter(_logger);
    }

    [Fact]
    public void SortBudsTopologicallyFromDependencyGraph_SortBudsTopologically_WhenThereAreNoDependencyCycles()
    {
        Dictionary<string, BudInfo> testBudsData = new()
        {
            ["8"] = CreateTestBudInfo("8", ["7", "3"]),
            ["2"] = CreateTestBudInfo("2", ["11"]),
            ["3"] = CreateTestBudInfo("3", []),
            ["10"] = CreateTestBudInfo("10", ["3", "11"]),
            ["5"] = CreateTestBudInfo("5", []),
            ["11"] = CreateTestBudInfo("11", ["5", "7"]),
            ["9"] = CreateTestBudInfo("9", ["8", "11"]),
            ["7"] = CreateTestBudInfo("7", [])
        };

        IList<BudInfo> sortedList = _sut.SortBudsTopologicallyFromDependencyGraph(testBudsData);

        sortedList.Should().HaveCount(8);

        List<string> seenBuds = new();
        foreach (BudInfo budInfo in sortedList)
        {
            List<string> dependencyIds = budInfo.BudManifest.BudDependencies.Select(d => d.BudId).ToList();
            dependencyIds.Should().AllSatisfy(d => d.Should().BeOneOf(seenBuds));
            testBudsData[budInfo.BudManifest.BudId].Should().BeEquivalentTo(budInfo);
            seenBuds.Add(budInfo.BudManifest.BudId);
        }
    }

    [Fact]
    public void SortBudsTopologicallyFromDependencyGraph_ThrowsException_WhenThereIsADependencyCycle()
    {
        Dictionary<string, BudInfo> testBudsData = new()
        {
            ["8"] = CreateTestBudInfo("8", ["7", "3"]),
            ["2"] = CreateTestBudInfo("2", ["11"]),
            ["3"] = CreateTestBudInfo("3", []),
            ["10"] = CreateTestBudInfo("10", ["3", "11"]),
            ["5"] = CreateTestBudInfo("5", []),
            ["11"] = CreateTestBudInfo("11", ["5", "7"]),
            ["9"] = CreateTestBudInfo("9", ["8", "11"]),
            ["7"] = CreateTestBudInfo("7", ["2"])
        };

        _sut.Invoking(sut => sut.SortBudsTopologicallyFromDependencyGraph(testBudsData))
            .Should().Throw<Exception>().WithMessage("*cyclic dependency detected*7*2*11*7*");
    }

    [Fact]
    public void SortBudsTopologicallyFromDependencyGraph_IgnoresMissingDependencies_WhenBudsHaveMissingDependencies()
    {
        Dictionary<string, BudInfo> testBudsData = new()
        {
            ["8"] = CreateTestBudInfo("8", ["7", "3"]),
            ["2"] = CreateTestBudInfo("2", ["11"]),
            ["3"] = CreateTestBudInfo("3", []),
            ["10"] = CreateTestBudInfo("10", ["3", "11"]),
            ["5"] = CreateTestBudInfo("5", ["20"]),
            ["11"] = CreateTestBudInfo("11", ["5", "7"]),
            ["9"] = CreateTestBudInfo("9", ["8", "11"]),
            ["7"] = CreateTestBudInfo("7", [])
        };

        IList<BudInfo> sortedList = _sut.SortBudsTopologicallyFromDependencyGraph(testBudsData);

        sortedList.Should().HaveCount(8);

        List<string> seenBuds = new();
        foreach (BudInfo budInfo in sortedList)
        {
            List<string> dependencyIds = budInfo.BudManifest.BudDependencies.Select(d => d.BudId).ToList();
            if (budInfo.BudManifest.BudId != "5")
                dependencyIds.Should().AllSatisfy(d => d.Should().BeOneOf(seenBuds));
            testBudsData[budInfo.BudManifest.BudId].Should().BeEquivalentTo(budInfo);
            seenBuds.Add(budInfo.BudManifest.BudId);
        }
    }

    private static BudInfo CreateTestBudInfo(string budId, string[] dependenciesIds)
    {
        return new BudInfo
        {
            BudManifest = new()
            {
                AssemblyName = budId,
                BudId = budId,
                BudName = "name",
                BudVersion = new(1, 0, 0),
                BudAuthor = "author",
                BudDependencies = dependenciesIds.Select(id => new BudDependency
                {
                    BudId = id,
                    Optional = false,
                    Version = new(new(0, 0, 0))
                }).ToArray(),
                BudIncompatibilities = []
            },
            BudAssemblyPath = $"{budId}.dll",
            BudType = new("namespace", budId, TypeAttributes.Class)
        };
    }
}