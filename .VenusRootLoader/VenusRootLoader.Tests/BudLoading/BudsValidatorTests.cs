using AsmResolver.PE.DotNet.Metadata.Tables;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using VenusRootLoader.BudLoading;

namespace VenusRootLoader.Tests.BudLoading;

public sealed class BudsValidatorTests
{
    private readonly FakeLogger<BudsValidator> _logger = new();

    private readonly IBudsValidator _sut;

    public BudsValidatorTests()
    {
        _sut = new BudsValidator(_logger);
    }

    [Fact]
    public void RemoveInvalidBuds_DoesNotRemoveAnyBuds_WhenAllBudsAreValid()
    {
        List<BudInfo> buds =
        [
            new()
            {
                BudManifest = new()
                {
                    AssemblyFile = "a",
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
                    AssemblyFile = "b",
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
                    AssemblyFile = "c",
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

        IDictionary<string, BudInfo> validBuds = _sut.RemoveInvalidBuds(buds);

        validBuds.Count.Should().Be(3);
        validBuds[buds[0].BudManifest.BudId].Should().BeEquivalentTo(buds[0]);
        validBuds[buds[1].BudManifest.BudId].Should().BeEquivalentTo(buds[1]);
        validBuds[buds[2].BudManifest.BudId].Should().BeEquivalentTo(buds[2]);
    }

    [Fact]
    public void RemoveInvalidBuds_RemoveOlderVersion_WhenTwoBudsHaveTheSameId()
    {
        string budDuplicatedId = "SameId";
        List<BudInfo> buds1 =
        [
            new()
            {
                BudManifest = new()
                {
                    AssemblyFile = "a",
                    BudId = budDuplicatedId,
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
                    AssemblyFile = "b",
                    BudId = budDuplicatedId,
                    BudName = "b",
                    BudVersion = new(2, 0, 0),
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
                    AssemblyFile = "c",
                    BudId = "DifferentId",
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
        List<BudInfo> buds = buds1;

        IDictionary<string, BudInfo> validBuds = _sut.RemoveInvalidBuds(buds);

        validBuds.Count.Should().Be(2);
        validBuds[budDuplicatedId].Should().BeEquivalentTo(buds[1]);
        validBuds[buds[2].BudManifest.BudId].Should().BeEquivalentTo(buds[2]);

        _logger.LatestRecord.Level.Should().Be(LogLevel.Warning);
        _logger.LatestRecord.Message.Should().MatchEquivalentOf(
            $"*{budDuplicatedId}*found multiple times*\n\n" +
            $"{buds[1].BudManifest.BudVersion} - {buds[1].BudAssemblyPath} - *selected\n" +
            $"{buds[0].BudManifest.BudVersion} - {buds[0].BudAssemblyPath} - *ignored");
    }

    [Fact]
    public void RemoveInvalidBuds_RemoveIncompatibleBuds_WhenBudsAreIncompatibleWithEachOther()
    {
        List<BudInfo> buds =
        [
            new()
            {
                BudManifest = new()
                {
                    AssemblyFile = "a",
                    BudId = "a",
                    BudName = "a",
                    BudVersion = new(1, 5, 0),
                    BudAuthor = "a",
                    BudDependencies = [],
                    BudIncompatibilities =
                    [
                        new()
                        {
                            BudId = "b",
                            Version = null
                        }
                    ]
                },
                BudAssemblyPath = "a.dll",
                BudType = new("a", "a", TypeAttributes.Class)
            },
            new()
            {
                BudManifest = new()
                {
                    AssemblyFile = "b",
                    BudId = "b",
                    BudName = "b",
                    BudVersion = new(1, 5, 0),
                    BudAuthor = "b",
                    BudDependencies = [],
                    BudIncompatibilities =
                    [
                        new()
                        {
                            BudId = "a",
                            Version = new(new(1, 0, 0), true, new(1, 5, 0), true)
                        }
                    ]
                },
                BudAssemblyPath = "b.dll",
                BudType = new("b", "b", TypeAttributes.Class)
            },
            new()
            {
                BudManifest = new()
                {
                    AssemblyFile = "c",
                    BudId = "c",
                    BudName = "c",
                    BudVersion = new(1, 5, 0),
                    BudAuthor = "c",
                    BudDependencies = [],
                    BudIncompatibilities =
                    [
                        new()
                        {
                            BudId = "b",
                            Version = new(new(1, 6, 0), true, new(1, 8, 0), true)
                        }
                    ]
                },
                BudAssemblyPath = "c.dll",
                BudType = new("c", "c", TypeAttributes.Class)
            }
        ];

        IDictionary<string, BudInfo> validBuds = _sut.RemoveInvalidBuds(buds);

        validBuds.Count.Should().Be(1);
        validBuds[buds[2].BudManifest.BudId].Should().BeEquivalentTo(buds[2]);

        IReadOnlyList<FakeLogRecord> logs = _logger.Collector.GetSnapshot();
        for (int i = 0; i < logs.Count; i++)
        {
            FakeLogRecord logRecord = logs[i];
            logRecord.Level.Should().Be(LogLevel.Error);
            logRecord.Message.Should().MatchEquivalentOf(
                $"*{buds[i].BudManifest.BudId}*not be loaded*incompatible*" +
                $"{buds[i].BudManifest.BudIncompatibilities[0].BudId}*version*" +
                $"{buds[i].BudManifest.BudVersion.ToFullString()}*version range*" +
                $"{buds[i].BudManifest.BudIncompatibilities[0].Version?.ToNormalizedString() ?? "any version"}");
        }
    }
}