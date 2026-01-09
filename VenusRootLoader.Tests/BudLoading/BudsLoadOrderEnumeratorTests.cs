using AsmResolver.PE.DotNet.Metadata.Tables;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using VenusRootLoader.BudLoading;

namespace VenusRootLoader.Tests.BudLoading;

public sealed class BudsLoadOrderEnumeratorTests
{
    private readonly FakeLogger<BudsLoadOrderEnumerator> _logger = new();

    private readonly BudsLoadOrderEnumerator _sut;

    public BudsLoadOrderEnumeratorTests()
    {
        _sut = new BudsLoadOrderEnumerator(_logger);
    }

    [Fact]
    public void EnumerateBudsWithFulfilledDependencies_EnumeratesSameBudsInOrder_WhenAllBudsHaveFulfilledDependencies()
    {
        List<BudInfo> buds =
        [
            new()
            {
                BudManifest = new()
                {
                    AssemblyName = "a",
                    BudId = "a",
                    BudName = "a",
                    BudVersion = new(1, 0, 0),
                    BudAuthor = "a",
                    BudDependencies = [],
                    BudIncompatibilities = []
                },
                BudAssemblyPath = "a.dll",
                BudType = new("a", "a", TypeAttributes.Class)
            },
            new()
            {
                BudManifest = new()
                {
                    AssemblyName = "b",
                    BudId = "b",
                    BudName = "b",
                    BudVersion = new(1, 0, 0),
                    BudAuthor = "b",
                    BudDependencies = [],
                    BudIncompatibilities = []
                },
                BudAssemblyPath = "b.dll",
                BudType = new("b", "b", TypeAttributes.Class)
            },
            new()
            {
                BudManifest = new()
                {
                    AssemblyName = "c",
                    BudId = "c",
                    BudName = "c",
                    BudVersion = new(1, 0, 0),
                    BudAuthor = "c",
                    BudDependencies = [],
                    BudIncompatibilities = []
                },
                BudAssemblyPath = "c.dll",
                BudType = new("c", "c", TypeAttributes.Class)
            }
        ];

        List<BudInfo> enumeratedBuds = _sut.EnumerateBudsWithFulfilledDependencies(buds).ToList();

        enumeratedBuds.Should().BeEquivalentTo(buds);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EnumerateBudsWithFulfilledDependencies_ReportsBuds_WhenBudsHaveMissingDependencies(bool isOptional)
    {
        List<BudInfo> buds =
        [
            new()
            {
                BudManifest = new()
                {
                    AssemblyName = "a",
                    BudId = "a",
                    BudName = "a",
                    BudVersion = new(1, 0, 0),
                    BudAuthor = "a",
                    BudDependencies =
                    [
                        new()
                        {
                            BudId = "d",
                            Optional = isOptional,
                            Version = new(new(1, 0, 0)),
                        }
                    ],
                    BudIncompatibilities = []
                },
                BudAssemblyPath = "a.dll",
                BudType = new("a", "a", TypeAttributes.Class)
            },
            new()
            {
                BudManifest = new()
                {
                    AssemblyName = "b",
                    BudId = "b",
                    BudName = "b",
                    BudVersion = new(1, 0, 0),
                    BudAuthor = "b",
                    BudDependencies = [],
                    BudIncompatibilities = []
                },
                BudAssemblyPath = "b.dll",
                BudType = new("b", "b", TypeAttributes.Class)
            },
            new()
            {
                BudManifest = new()
                {
                    AssemblyName = "c",
                    BudId = "c",
                    BudName = "c",
                    BudVersion = new(1, 0, 0),
                    BudAuthor = "c",
                    BudDependencies = [],
                    BudIncompatibilities = []
                },
                BudAssemblyPath = "c.dll",
                BudType = new("c", "c", TypeAttributes.Class)
            }
        ];

        List<BudInfo> enumeratedBuds = _sut.EnumerateBudsWithFulfilledDependencies(buds).ToList();

        enumeratedBuds.Should().BeEquivalentTo(isOptional ? buds : buds.Skip(1));

        _logger.Collector.Count.Should().Be(1);
        _logger.LatestRecord.Level.Should().Be(isOptional ? LogLevel.Warning : LogLevel.Error);
        _logger.LatestRecord.Message.Should().MatchEquivalentOf(
            $"*{buds[0].BudManifest.BudId}*{(isOptional ? "will" : "not")} be loaded*unsatisfied " +
            $"{(isOptional ? "optional dependencies*" : "dependencies*not optional*")}" +
            $"{buds[0].BudManifest.BudDependencies[0].BudId} - {(isOptional ? "Optional" : "Required")} - *missing*");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EnumerateBudsWithFulfilledDependencies_ReportsBuds_WhenBudsHaveOutOfVersionRangesDependencies(
        bool isOptional)
    {
        List<BudInfo> buds =
        [
            new()
            {
                BudManifest = new()
                {
                    AssemblyName = "a",
                    BudId = "a",
                    BudName = "a",
                    BudVersion = new(1, 0, 0),
                    BudAuthor = "a",
                    BudDependencies = [],
                    BudIncompatibilities = []
                },
                BudAssemblyPath = "a.dll",
                BudType = new("a", "a", TypeAttributes.Class)
            },
            new()
            {
                BudManifest = new()
                {
                    AssemblyName = "b",
                    BudId = "b",
                    BudName = "b",
                    BudVersion = new(1, 0, 0),
                    BudAuthor = "b",
                    BudDependencies =
                    [
                        new()
                        {
                            BudId = "a",
                            Optional = isOptional,
                            Version = new(new(1, 1, 0)),
                        }
                    ],
                    BudIncompatibilities = []
                },
                BudAssemblyPath = "b.dll",
                BudType = new("b", "b", TypeAttributes.Class)
            },
            new()
            {
                BudManifest = new()
                {
                    AssemblyName = "c",
                    BudId = "c",
                    BudName = "c",
                    BudVersion = new(1, 0, 0),
                    BudAuthor = "c",
                    BudDependencies = [],
                    BudIncompatibilities = []
                },
                BudAssemblyPath = "c.dll",
                BudType = new("c", "c", TypeAttributes.Class)
            }
        ];

        List<BudInfo> enumeratedBuds = _sut.EnumerateBudsWithFulfilledDependencies(buds).ToList();

        enumeratedBuds.Should().BeEquivalentTo(isOptional ? buds : buds.Except([buds[1]]));

        _logger.Collector.Count.Should().Be(1);
        _logger.LatestRecord.Level.Should().Be(isOptional ? LogLevel.Warning : LogLevel.Error);
        _logger.LatestRecord.Message.Should().MatchEquivalentOf(
            $"*{buds[1].BudManifest.BudId}*{(isOptional ? "will" : "not")} be loaded*unsatisfied " +
            $"{(isOptional ? "optional dependencies*" : "dependencies*not optional*")}" +
            $"{buds[1].BudManifest.BudDependencies[0].BudId} - {(isOptional ? "Optional" : "Required")} - *" +
            $"loaded*version*{buds[0].BudManifest.BudVersion.ToFullString()}*not satisfy*version range*" +
            $"{buds[1].BudManifest.BudDependencies[0].Version.ToNormalizedString()}*");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EnumerateBudsWithFulfilledDependencies_ReportsBuds_WhenBudsHaveUnfulfilledRequiredDependencies(
        bool isOptional)
    {
        List<BudInfo> buds =
        [
            new()
            {
                BudManifest = new()
                {
                    AssemblyName = "a",
                    BudId = "a",
                    BudName = "a",
                    BudVersion = new(1, 0, 0),
                    BudAuthor = "a",
                    BudDependencies =
                    [
                        new()
                        {
                            BudId = "d",
                            Optional = false,
                            Version = new(new(1, 0, 0)),
                        }
                    ],
                    BudIncompatibilities = []
                },
                BudAssemblyPath = "a.dll",
                BudType = new("a", "a", TypeAttributes.Class)
            },
            new()
            {
                BudManifest = new()
                {
                    AssemblyName = "b",
                    BudId = "b",
                    BudName = "b",
                    BudVersion = new(1, 0, 0),
                    BudAuthor = "b",
                    BudDependencies =
                    [
                        new()
                        {
                            BudId = "a",
                            Optional = isOptional,
                            Version = new(new(1, 0, 0)),
                        }
                    ],
                    BudIncompatibilities = []
                },
                BudAssemblyPath = "b.dll",
                BudType = new("b", "b", TypeAttributes.Class)
            },
            new()
            {
                BudManifest = new()
                {
                    AssemblyName = "c",
                    BudId = "c",
                    BudName = "c",
                    BudVersion = new(1, 0, 0),
                    BudAuthor = "c",
                    BudDependencies = [],
                    BudIncompatibilities = []
                },
                BudAssemblyPath = "c.dll",
                BudType = new("c", "c", TypeAttributes.Class)
            }
        ];

        List<BudInfo> enumeratedBuds = _sut.EnumerateBudsWithFulfilledDependencies(buds).ToList();

        enumeratedBuds.Should().BeEquivalentTo(isOptional ? buds.Skip(1) : buds.Skip(2));

        IReadOnlyList<FakeLogRecord> logs = _logger.Collector.GetSnapshot();
        logs.Count.Should().Be(2);
        logs[0].Level.Should().Be(LogLevel.Error);
        logs[0].Message.Should().MatchEquivalentOf(
            $"*{buds[0].BudManifest.BudId}*not be loaded*unsatisfied dependencies*not optional*" +
            $"{buds[0].BudManifest.BudDependencies[0].BudId} - Required - *missing*");

        logs[1].Level.Should().Be(isOptional ? LogLevel.Warning : LogLevel.Error);
        logs[1].Message.Should().MatchEquivalentOf(
            $"*{buds[1].BudManifest.BudId}*{(isOptional ? "will" : "not")} be loaded*unsatisfied " +
            $"{(isOptional ? "optional dependencies*" : "dependencies*not optional*")}" +
            $"{buds[1].BudManifest.BudDependencies[0].BudId} - {(isOptional ? "Optional" : "Required")} - *unsatisfied dependencies*");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MarkBudAsFailedDuringLoad_ReportsErrors_WhenBudsHasFailedBudAsDependency(bool isOptional)
    {
        List<BudInfo> buds =
        [
            new()
            {
                BudManifest = new()
                {
                    AssemblyName = "a",
                    BudId = "a",
                    BudName = "a",
                    BudVersion = new(1, 0, 0),
                    BudAuthor = "a",
                    BudDependencies = [],
                    BudIncompatibilities = []
                },
                BudAssemblyPath = "a.dll",
                BudType = new("a", "a", TypeAttributes.Class)
            },
            new()
            {
                BudManifest = new()
                {
                    AssemblyName = "b",
                    BudId = "b",
                    BudName = "b",
                    BudVersion = new(1, 0, 0),
                    BudAuthor = "b",
                    BudDependencies =
                    [
                        new()
                        {
                            BudId = "a",
                            Optional = isOptional,
                            Version = new(new(1, 0, 0)),
                        }
                    ],
                    BudIncompatibilities = []
                },
                BudAssemblyPath = "b.dll",
                BudType = new("b", "b", TypeAttributes.Class)
            },
            new()
            {
                BudManifest = new()
                {
                    AssemblyName = "c",
                    BudId = "c",
                    BudName = "c",
                    BudVersion = new(1, 0, 0),
                    BudAuthor = "c",
                    BudDependencies = [],
                    BudIncompatibilities = []
                },
                BudAssemblyPath = "c.dll",
                BudType = new("c", "c", TypeAttributes.Class)
            }
        ];

        int index = 0;
        List<BudInfo> loadedBuds = new();
        foreach (BudInfo bud in _sut.EnumerateBudsWithFulfilledDependencies(buds))
        {
            if (index == 0)
                _sut.MarkBudAsFailedDuringLoad(bud);
            else
                loadedBuds.Add(bud);
            index++;
        }

        loadedBuds.Should().BeEquivalentTo(isOptional ? buds.Skip(1) : buds.Skip(2));

        _logger.Collector.Count.Should().Be(1);
        _logger.LatestRecord.Level.Should().Be(isOptional ? LogLevel.Warning : LogLevel.Error);
        _logger.LatestRecord.Message.Should().MatchEquivalentOf(
            $"*{buds[1].BudManifest.BudId}*{(isOptional ? "will" : "not")} be loaded*unsatisfied " +
            $"{(isOptional ? "optional dependencies*" : "dependencies*not optional*")}" +
            $"{buds[1].BudManifest.BudDependencies[0].BudId} - {(isOptional ? "Optional" : "Required")} - *threw an exception*");
    }
}