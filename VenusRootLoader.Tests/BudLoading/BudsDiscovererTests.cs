using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NuGet.Versioning;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using VenusRootLoader.Api;
using VenusRootLoader.BudLoading;
using VenusRootLoader.JsonConverters;

namespace VenusRootLoader.Tests.BudLoading;

public sealed class BudsDiscovererTests
{
    private enum BudManifestFileState
    {
        Valid,
        Missing,
        Corrupt,
        ContainsNull
    }

    private enum BudManifestDependencyState
    {
        Valid,
        EmptyBudId,
        SelfReference,
        NullVersionRange
    }

    private enum BudManifestIncompatibilityState
    {
        Valid,
        EmptyBudId,
        SelfReference
    }

    private static readonly string RootPath = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "/" : "C:\\";
    private static readonly string BudsPath = Path.Combine(RootPath, "Buds");
    private static readonly string ConfigPath = Path.Combine(RootPath, "Config");
    private static readonly string LoaderPath = Path.Combine(RootPath, nameof(VenusRootLoader));

    private readonly MockFileSystem _fileSystem = new();
    private readonly FakeLogger<BudsDiscoverer> _logger = new();

    private readonly BudLoaderContext _budLoaderContext = new()
    {
        BudsPath = BudsPath,
        ConfigPath = ConfigPath,
        LoaderPath = LoaderPath
    };

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        Converters =
        {
            NuGetVersionJsonConverter.Instance,
            NuGetVersionRangeJsonConverter.Instance
        }
    };

    private readonly BudsDiscoverer _sut;

    public BudsDiscovererTests()
    {
        _fileSystem.Directory.CreateDirectory(BudsPath);
        _fileSystem.Directory.CreateDirectory(ConfigPath);
        _fileSystem.Directory.CreateDirectory(LoaderPath);

        _sut = new BudsDiscoverer(_fileSystem, _logger, _budLoaderContext);
    }

    [Fact]
    public void DiscoverAllBudsFromDisk_ReturnsEmptyList_WhenThereAreNoBuds()
    {
        IList<BudInfo> buds = _sut.DiscoverAllBudsFromDisk();

        AssertErrorLogs(0);

        buds.Should().BeEmpty();
    }

    [Fact]
    public void DiscoverAllBudsFromDisk_ReturnsAllBuds_WhenBudsAreValid()
    {
        BudManifest budManifestA = CreateBudManifest("a", BudManifestFileState.Valid);
        CreateAssemblyForBud(budManifestA);
        BudManifest budManifestB = CreateBudManifest("b", BudManifestFileState.Valid);
        CreateAssemblyForBud(budManifestB);
        BudManifest budManifestC = CreateBudManifest("c", BudManifestFileState.Valid);
        CreateAssemblyForBud(budManifestC);

        IList<BudInfo> buds = _sut.DiscoverAllBudsFromDisk();

        AssertErrorLogs(0);

        buds.Should().HaveCount(3);
        buds[0].BudAssemblyPath.Should().Be(Path.Combine(BudsPath, "a", "a.dll"));
        buds[0].BudType.Name!.Value.Should().Be("aType0");
        buds[0].BudManifest.Should().BeEquivalentTo(budManifestA);
        buds[1].BudAssemblyPath.Should().Be(Path.Combine(BudsPath, "b", "b.dll"));
        buds[1].BudType.Name!.Value.Should().Be("bType0");
        buds[1].BudManifest.Should().BeEquivalentTo(budManifestB);
        buds[2].BudAssemblyPath.Should().Be(Path.Combine(BudsPath, "c", "c.dll"));
        buds[2].BudType.Name!.Value.Should().Be("cType0");
        buds[2].BudManifest.Should().BeEquivalentTo(budManifestC);
    }

    [Fact]
    public void DiscoverAllBudsFromDisk_SkipsBuds_WhenBudsHaveNoManifests()
    {
        BudManifest budManifestA = CreateBudManifest("a", BudManifestFileState.Missing);
        CreateAssemblyForBud(budManifestA);
        BudManifest budManifestB = CreateBudManifest("b", BudManifestFileState.Valid);
        CreateAssemblyForBud(budManifestB);

        IList<BudInfo> buds = _sut.DiscoverAllBudsFromDisk();

        AssertErrorLogs(0);

        buds.Should().ContainSingle();
        buds[0].BudAssemblyPath.Should().Be(Path.Combine(BudsPath, "b", "b.dll"));
        buds[0].BudType.Name!.Value.Should().Be("bType0");
        buds[0].BudManifest.Should().BeEquivalentTo(budManifestB);
    }

    [Fact]
    public void DiscoverAllBudsFromDisk_SkipsBudsAndLogsError_WhenBudsHaveNullManifest()
    {
        BudManifest budManifestA = CreateBudManifest("a", BudManifestFileState.ContainsNull);
        CreateAssemblyForBud(budManifestA);
        BudManifest budManifestB = CreateBudManifest("b", BudManifestFileState.Valid);
        CreateAssemblyForBud(budManifestB);

        IList<BudInfo> buds = _sut.DiscoverAllBudsFromDisk();

        AssertErrorLogs(1, "*deserialized*null*");

        buds.Should().ContainSingle();
        buds[0].BudAssemblyPath.Should().Be(Path.Combine(BudsPath, "b", "b.dll"));
        buds[0].BudType.Name!.Value.Should().Be("bType0");
        buds[0].BudManifest.Should().BeEquivalentTo(budManifestB);
    }

    [Fact]
    public void DiscoverAllBudsFromDisk_SkipsBudsAndLogsError_WhenBudsHaveCorruptManifest()
    {
        BudManifest budManifestA = CreateBudManifest("a", BudManifestFileState.Corrupt);
        CreateAssemblyForBud(budManifestA);
        BudManifest budManifestB = CreateBudManifest("b", BudManifestFileState.Valid);
        CreateAssemblyForBud(budManifestB);

        IList<BudInfo> buds = _sut.DiscoverAllBudsFromDisk();

        AssertErrorLogs(1);

        buds.Should().ContainSingle();
        buds[0].BudAssemblyPath.Should().Be(Path.Combine(BudsPath, "b", "b.dll"));
        buds[0].BudType.Name!.Value.Should().Be("bType0");
        buds[0].BudManifest.Should().BeEquivalentTo(budManifestB);
    }

    [Fact]
    public void DiscoverAllBudsFromDisk_SkipsBudsAndLogError_WhenBudsHaveNoAssemblyFileNameInManifest()
    {
        BudManifest budManifestA = CreateBudManifest("a", BudManifestFileState.Valid, emptyAssemblyFileName: true);
        CreateAssemblyForBud(budManifestA);
        BudManifest budManifestB = CreateBudManifest("b", BudManifestFileState.Valid);
        CreateAssemblyForBud(budManifestB);

        IList<BudInfo> buds = _sut.DiscoverAllBudsFromDisk();

        AssertErrorLogs(1, $"{nameof(BudManifest.AssemblyName)} is not specified");

        buds.Should().ContainSingle();
        buds[0].BudAssemblyPath.Should().Be(Path.Combine(BudsPath, "b", "b.dll"));
        buds[0].BudType.Name!.Value.Should().Be("bType0");
        buds[0].BudManifest.Should().BeEquivalentTo(budManifestB);
    }

    [Fact]
    public void DiscoverAllBudsFromDisk_SkipsBudsAndLogError_WhenBudsHaveNoBudIdInManifest()
    {
        BudManifest budManifestA = CreateBudManifest("a", BudManifestFileState.Valid, emptyBudId: true);
        CreateAssemblyForBud(budManifestA);
        BudManifest budManifestB = CreateBudManifest("b", BudManifestFileState.Valid);
        CreateAssemblyForBud(budManifestB);

        IList<BudInfo> buds = _sut.DiscoverAllBudsFromDisk();

        AssertErrorLogs(1, $"{nameof(BudManifest.BudId)} is not specified");

        buds.Should().ContainSingle();
        buds[0].BudAssemblyPath.Should().Be(Path.Combine(BudsPath, "b", "b.dll"));
        buds[0].BudType.Name!.Value.Should().Be("bType0");
        buds[0].BudManifest.Should().BeEquivalentTo(budManifestB);
    }

    [Fact]
    public void DiscoverAllBudsFromDisk_SkipsBudsAndLogError_WhenBudsHaveNoBudNameInManifest()
    {
        BudManifest budManifestA = CreateBudManifest("a", BudManifestFileState.Valid, emptyBudName: true);
        CreateAssemblyForBud(budManifestA);
        BudManifest budManifestB = CreateBudManifest("b", BudManifestFileState.Valid);
        CreateAssemblyForBud(budManifestB);

        IList<BudInfo> buds = _sut.DiscoverAllBudsFromDisk();

        AssertErrorLogs(1, $"{nameof(BudManifest.BudName)} is not specified");

        buds.Should().ContainSingle();
        buds[0].BudAssemblyPath.Should().Be(Path.Combine(BudsPath, "b", "b.dll"));
        buds[0].BudType.Name!.Value.Should().Be("bType0");
        buds[0].BudManifest.Should().BeEquivalentTo(budManifestB);
    }

    [Fact]
    public void DiscoverAllBudsFromDisk_SkipsBudsAndLogError_WhenBudsHaveNoBudAuthorInManifest()
    {
        BudManifest budManifestA = CreateBudManifest("a", BudManifestFileState.Valid, emptyBudAuthor: true);
        CreateAssemblyForBud(budManifestA);
        BudManifest budManifestB = CreateBudManifest("b", BudManifestFileState.Valid);
        CreateAssemblyForBud(budManifestB);

        IList<BudInfo> buds = _sut.DiscoverAllBudsFromDisk();

        AssertErrorLogs(1, $"{nameof(BudManifest.BudAuthor)} is not specified");

        buds.Should().ContainSingle();
        buds[0].BudAssemblyPath.Should().Be(Path.Combine(BudsPath, "b", "b.dll"));
        buds[0].BudType.Name!.Value.Should().Be("bType0");
        buds[0].BudManifest.Should().BeEquivalentTo(budManifestB);
    }

    [Fact]
    public void DiscoverAllBudsFromDisk_SkipsBudsAndLogError_WhenBudsHaveNullBudVersionInManifest()
    {
        BudManifest budManifestA = CreateBudManifest("a", BudManifestFileState.Valid, nullBudVersion: true);
        CreateAssemblyForBud(budManifestA);
        BudManifest budManifestB = CreateBudManifest("b", BudManifestFileState.Valid);
        CreateAssemblyForBud(budManifestB);

        IList<BudInfo> buds = _sut.DiscoverAllBudsFromDisk();

        AssertErrorLogs(1, $"{nameof(BudManifest.BudVersion)} is null");

        buds.Should().ContainSingle();
        buds[0].BudAssemblyPath.Should().Be(Path.Combine(BudsPath, "b", "b.dll"));
        buds[0].BudType.Name!.Value.Should().Be("bType0");
        buds[0].BudManifest.Should().BeEquivalentTo(budManifestB);
    }

    [Fact]
    public void DiscoverAllBudsFromDisk_SkipsBudsAndLogError_WhenBudsHaveNoBudIdInDependencyOfManifest()
    {
        BudManifest budManifestA = CreateBudManifest(
            "a",
            BudManifestFileState.Valid,
            dependencyState: BudManifestDependencyState.EmptyBudId);
        CreateAssemblyForBud(budManifestA);
        BudManifest budManifestB = CreateBudManifest("b", BudManifestFileState.Valid);
        CreateAssemblyForBud(budManifestB);

        IList<BudInfo> buds = _sut.DiscoverAllBudsFromDisk();

        AssertErrorLogs(1, "At least one dependency has an unspecified bud ID");

        buds.Should().ContainSingle();
        buds[0].BudAssemblyPath.Should().Be(Path.Combine(BudsPath, "b", "b.dll"));
        buds[0].BudType.Name!.Value.Should().Be("bType0");
        buds[0].BudManifest.Should().BeEquivalentTo(budManifestB);
    }

    [Fact]
    public void DiscoverAllBudsFromDisk_SkipsBudsAndLogError_WhenBudsHaveNoBudIdInIncompatibilityOfManifest()
    {
        BudManifest budManifestA = CreateBudManifest(
            "a",
            BudManifestFileState.Valid,
            incompatibilityState: BudManifestIncompatibilityState.EmptyBudId);
        CreateAssemblyForBud(budManifestA);
        BudManifest budManifestB = CreateBudManifest("b", BudManifestFileState.Valid);
        CreateAssemblyForBud(budManifestB);

        IList<BudInfo> buds = _sut.DiscoverAllBudsFromDisk();

        AssertErrorLogs(1, "At least one incompatibility has an unspecified bud ID");

        buds.Should().ContainSingle();
        buds[0].BudAssemblyPath.Should().Be(Path.Combine(BudsPath, "b", "b.dll"));
        buds[0].BudType.Name!.Value.Should().Be("bType0");
        buds[0].BudManifest.Should().BeEquivalentTo(budManifestB);
    }

    [Fact]
    public void DiscoverAllBudsFromDisk_SkipsBudsAndLogError_WhenBudsHaveDependencyWithThemselves()
    {
        BudManifest budManifestA = CreateBudManifest(
            "a",
            BudManifestFileState.Valid,
            dependencyState: BudManifestDependencyState.SelfReference);
        CreateAssemblyForBud(budManifestA);
        BudManifest budManifestB = CreateBudManifest("b", BudManifestFileState.Valid);
        CreateAssemblyForBud(budManifestB);

        IList<BudInfo> buds = _sut.DiscoverAllBudsFromDisk();

        AssertErrorLogs(1, "The bud cannot have a dependency with itself");

        buds.Should().ContainSingle();
        buds[0].BudAssemblyPath.Should().Be(Path.Combine(BudsPath, "b", "b.dll"));
        buds[0].BudType.Name!.Value.Should().Be("bType0");
        buds[0].BudManifest.Should().BeEquivalentTo(budManifestB);
    }

    [Fact]
    public void DiscoverAllBudsFromDisk_SkipsBudsAndLogError_WhenBudsHaveIncompatibilityWithThemselves()
    {
        BudManifest budManifestA = CreateBudManifest(
            "a",
            BudManifestFileState.Valid,
            incompatibilityState: BudManifestIncompatibilityState.SelfReference);
        CreateAssemblyForBud(budManifestA);
        BudManifest budManifestB = CreateBudManifest("b", BudManifestFileState.Valid);
        CreateAssemblyForBud(budManifestB);

        IList<BudInfo> buds = _sut.DiscoverAllBudsFromDisk();

        AssertErrorLogs(1, "The bud cannot have an incompatibility with itself");

        buds.Should().ContainSingle();
        buds[0].BudAssemblyPath.Should().Be(Path.Combine(BudsPath, "b", "b.dll"));
        buds[0].BudType.Name!.Value.Should().Be("bType0");
        buds[0].BudManifest.Should().BeEquivalentTo(budManifestB);
    }

    [Fact]
    public void DiscoverAllBudsFromDisk_SkipsBudsAndLogError_WhenBudsHaveDependencyWithNoVersionRange()
    {
        BudManifest budManifestA = CreateBudManifest(
            "a",
            BudManifestFileState.Valid,
            dependencyState: BudManifestDependencyState.NullVersionRange);
        CreateAssemblyForBud(budManifestA);
        BudManifest budManifestB = CreateBudManifest("b", BudManifestFileState.Valid);
        CreateAssemblyForBud(budManifestB);

        IList<BudInfo> buds = _sut.DiscoverAllBudsFromDisk();

        string dependencyBudId = buds[0].BudManifest.BudDependencies[0].BudId;
        AssertErrorLogs(1, $"The dependency {dependencyBudId} has a null {nameof(BudDependency.Version)}");

        buds.Should().ContainSingle();
        buds[0].BudAssemblyPath.Should().Be(Path.Combine(BudsPath, "b", "b.dll"));
        buds[0].BudType.Name!.Value.Should().Be("bType0");
        buds[0].BudManifest.Should().BeEquivalentTo(budManifestB);
    }

    [Fact]
    public void DiscoverAllBudsFromDisk_SkipsBudsAndLogError_WhenBudsHaveNoAssembly()
    {
        CreateBudManifest("a", BudManifestFileState.Valid);
        BudManifest budManifestB = CreateBudManifest("b", BudManifestFileState.Valid);
        CreateAssemblyForBud(budManifestB);

        IList<BudInfo> buds = _sut.DiscoverAllBudsFromDisk();

        AssertErrorLogs(1, "*assembly*not exist*");

        buds.Should().ContainSingle();
        buds[0].BudAssemblyPath.Should().Be(Path.Combine(BudsPath, "b", "b.dll"));
        buds[0].BudType.Name!.Value.Should().Be("bType0");
        buds[0].BudManifest.Should().BeEquivalentTo(budManifestB);
    }

    [Fact]
    public void DiscoverAllBudsFromDisk_SkipsBudsAndLogError_WhenBudsHaveCorruptAssembly()
    {
        BudManifest budManifestA = CreateBudManifest("a", BudManifestFileState.Valid);
        CreateAssemblyForBud(budManifestA, fileIsCorrupt: true);
        BudManifest budManifestB = CreateBudManifest("b", BudManifestFileState.Valid);
        CreateAssemblyForBud(budManifestB);

        IList<BudInfo> buds = _sut.DiscoverAllBudsFromDisk();

        AssertErrorLogs(1);

        buds.Should().ContainSingle();
        buds[0].BudAssemblyPath.Should().Be(Path.Combine(BudsPath, "b", "b.dll"));
        buds[0].BudType.Name!.Value.Should().Be("bType0");
        buds[0].BudManifest.Should().BeEquivalentTo(budManifestB);
    }

    [Fact]
    public void DiscoverAllBudsFromDisk_SkipsBudsAndLogError_WhenBudsAssemblyHaveNoBudType()
    {
        BudManifest budManifestA = CreateBudManifest("a", BudManifestFileState.Valid);
        CreateAssemblyForBud(budManifestA, hasNoBudType: true);
        BudManifest budManifestB = CreateBudManifest("b", BudManifestFileState.Valid);
        CreateAssemblyForBud(budManifestB);

        IList<BudInfo> buds = _sut.DiscoverAllBudsFromDisk();

        AssertErrorLogs(1, $"*no non abstract classes*inherits from {nameof(Bud)}*parameterless constructor*");

        buds.Should().ContainSingle();
        buds[0].BudAssemblyPath.Should().Be(Path.Combine(BudsPath, "b", "b.dll"));
        buds[0].BudType.Name!.Value.Should().Be("bType0");
        buds[0].BudManifest.Should().BeEquivalentTo(budManifestB);
    }

    [Fact]
    public void DiscoverAllBudsFromDisk_SkipsBudsAndLogError_WhenBudsTypeIsAbstract()
    {
        BudManifest budManifestA = CreateBudManifest("a", BudManifestFileState.Valid);
        CreateAssemblyForBud(budManifestA, budTypeIsAbstract: true);
        BudManifest budManifestB = CreateBudManifest("b", BudManifestFileState.Valid);
        CreateAssemblyForBud(budManifestB);

        IList<BudInfo> buds = _sut.DiscoverAllBudsFromDisk();

        AssertErrorLogs(1, $"*no non abstract classes*inherits from {nameof(Bud)}*parameterless constructor*");

        buds.Should().ContainSingle();
        buds[0].BudAssemblyPath.Should().Be(Path.Combine(BudsPath, "b", "b.dll"));
        buds[0].BudType.Name!.Value.Should().Be("bType0");
        buds[0].BudManifest.Should().BeEquivalentTo(budManifestB);
    }

    [Fact]
    public void DiscoverAllBudsFromDisk_SkipsBudsAndLogError_WhenBudsTypeHasNoParameterlessConstructor()
    {
        BudManifest budManifestA = CreateBudManifest("a", BudManifestFileState.Valid);
        CreateAssemblyForBud(budManifestA, budTypeHasNoParameterLessConstructor: true);
        BudManifest budManifestB = CreateBudManifest("b", BudManifestFileState.Valid);
        CreateAssemblyForBud(budManifestB);

        IList<BudInfo> buds = _sut.DiscoverAllBudsFromDisk();

        AssertErrorLogs(1, $"*no non abstract classes*inherits from {nameof(Bud)}*parameterless constructor*");

        buds.Should().ContainSingle();
        buds[0].BudAssemblyPath.Should().Be(Path.Combine(BudsPath, "b", "b.dll"));
        buds[0].BudType.Name!.Value.Should().Be("bType0");
        buds[0].BudManifest.Should().BeEquivalentTo(budManifestB);
    }

    [Fact]
    public void DiscoverAllBudsFromDisk_SkipsBudsAndLogError_WhenBudsHasMoreThanOneBudType()
    {
        BudManifest budManifestA = CreateBudManifest("a", BudManifestFileState.Valid);
        CreateAssemblyForBud(budManifestA, hasMoreThanOneValidBudType: true);
        BudManifest budManifestB = CreateBudManifest("b", BudManifestFileState.Valid);
        CreateAssemblyForBud(budManifestB);

        IList<BudInfo> buds = _sut.DiscoverAllBudsFromDisk();

        AssertErrorLogs(
            1,
            $"*more than 1*non abstract classes*inherits from {nameof(Bud)}*parameterless constructor*aType0*aType1*");

        buds.Should().ContainSingle();
        buds[0].BudAssemblyPath.Should().Be(Path.Combine(BudsPath, "b", "b.dll"));
        buds[0].BudType.Name!.Value.Should().Be("bType0");
        buds[0].BudManifest.Should().BeEquivalentTo(budManifestB);
    }

    private void AssertErrorLogs(int expectedErrorLogsAmount, string expectedExceptionMessageTemplate = "*")
    {
        using AssertionScope scope = new();
        List<FakeLogRecord> errorLogs = _logger.Collector.GetSnapshot().Where(l => l.Level == LogLevel.Error).ToList();
        errorLogs.Should().HaveCount(expectedErrorLogsAmount);
        if (!scope.HasFailures())
            return;

        foreach (FakeLogRecord log in errorLogs)
        {
            TestContext.Current.TestOutputHelper!.WriteLine(log.Message);
            if (log.Exception is null)
                continue;

            log.Exception.Message.Should().MatchEquivalentOf(expectedExceptionMessageTemplate);
            TestContext.Current.TestOutputHelper!.WriteLine(log.Exception.ToString());
        }
    }

    private void CreateAssemblyForBud(
        BudManifest budManifest,
        bool fileIsCorrupt = false,
        bool hasNoBudType = false,
        bool budTypeIsAbstract = false,
        bool budTypeHasNoParameterLessConstructor = false,
        bool hasMoreThanOneValidBudType = false)
    {
        string budId = budManifest.BudId;

        using FileSystemStream assemblyStream = _fileSystem.File.Create(Path.Combine(BudsPath, budId, $"{budId}.dll"));
        if (fileIsCorrupt)
        {
            byte[] corruptBytes = "12345SomeNonSense54321"u8.ToArray();
            assemblyStream.Write(corruptBytes, 0, corruptBytes.Length);
            return;
        }

        DotNetRuntimeInfo net472RuntimeInfo = DotNetRuntimeInfo.Parse(".NETFramework,Version=v4.7.2");
        ModuleDefinition budModuleDefinition = new(new(budId), net472RuntimeInfo.GetDefaultCorLib());

        int amountOfValidBudType = hasNoBudType ? 0 : hasMoreThanOneValidBudType ? 2 : 1;
        for (int i = 0; i < amountOfValidBudType; i++)
        {
            ITypeDefOrRef baseBudTypeReference = budModuleDefinition.DefaultImporter.ImportType(typeof(Bud));
            TypeDefinition budType = new(
                "Namespace",
                $"{budId}Type{i}",
                budTypeIsAbstract
                    ? TypeAttributes.Abstract | TypeAttributes.Class
                    : TypeAttributes.Class,
                baseBudTypeReference);
            if (!budTypeHasNoParameterLessConstructor)
            {
                MethodDefinition budTypeCtor = MethodDefinition.CreateConstructor(budModuleDefinition);
                budType.Methods.Add(budTypeCtor);
            }

            budModuleDefinition.TopLevelTypes.Add(budType);
        }

        budModuleDefinition.Write(assemblyStream);
    }

    private BudManifest CreateBudManifest(
        string budId,
        BudManifestFileState manifestFileState,
        bool emptyAssemblyFileName = false,
        bool emptyBudId = false,
        bool emptyBudName = false,
        bool nullBudVersion = false,
        bool emptyBudAuthor = false,
        BudManifestDependencyState dependencyState = BudManifestDependencyState.Valid,
        BudManifestIncompatibilityState incompatibilityState = BudManifestIncompatibilityState.Valid)
    {
        _fileSystem.Directory.CreateDirectory(Path.Combine(BudsPath, budId));

        BudManifest budManifest = new()
        {
            AssemblyName = emptyAssemblyFileName ? "" : $"{budId}.dll",
            BudId = emptyBudId ? "" : budId,
            BudName = emptyBudName ? "" : $"{budId} name",
            BudVersion = nullBudVersion ? null! : new(1, 0, 0),
            BudAuthor = emptyBudAuthor ? "" : $"{budId} author",
            BudDependencies =
            [
                new()
                {
                    BudId = dependencyState switch
                    {
                        BudManifestDependencyState.EmptyBudId => "",
                        BudManifestDependencyState.SelfReference => budId,
                        _ => "SomeDependency"
                    },
                    Optional = false,
                    Version = dependencyState switch
                    {
                        BudManifestDependencyState.NullVersionRange => null!,
                        _ => VersionRange.Parse("1.0.0")
                    },
                }
            ],
            BudIncompatibilities =
            [
                new()
                {
                    BudId = incompatibilityState switch
                    {
                        BudManifestIncompatibilityState.EmptyBudId => "",
                        BudManifestIncompatibilityState.SelfReference => budId,
                        _ => "SomeIncompatibility"
                    },
                    Version = VersionRange.Parse("1.0.0")
                }
            ]
        };

        if (manifestFileState == BudManifestFileState.Missing)
            return budManifest;

        FileSystemStream manifestStream = _fileSystem.File.Create(Path.Combine(BudsPath, budId, "manifest.json"));
        string budManifestJson = manifestFileState switch
        {
            BudManifestFileState.Valid => JsonSerializer.Serialize(budManifest, _jsonSerializerOptions),
            BudManifestFileState.ContainsNull => "null",
            BudManifestFileState.Corrupt => "1234SomeNonsense54321",
            _ => throw new InvalidOperationException()
        };

        byte[] budManifestJsonBytes = Encoding.UTF8.GetBytes(budManifestJson);
        manifestStream.Write(budManifestJsonBytes, 0, budManifestJsonBytes.Length);
        manifestStream.Dispose();

        return budManifest;
    }
}