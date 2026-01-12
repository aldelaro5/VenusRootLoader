using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using VenusRootLoader.Api;
using VenusRootLoader.BudLoading;
using CustomAttributeNamedArgument = AsmResolver.DotNet.Signatures.CustomAttributeNamedArgument;
using MethodAttributes = AsmResolver.PE.DotNet.Metadata.Tables.MethodAttributes;
using SecurityAction = AsmResolver.PE.DotNet.Metadata.Tables.SecurityAction;
using SecurityAttribute = AsmResolver.DotNet.Signatures.SecurityAttribute;
using TypeAttributes = AsmResolver.PE.DotNet.Metadata.Tables.TypeAttributes;

namespace VenusRootLoader.Tests.BudLoading;

public sealed class BudLoaderTests
{
    private static readonly string RootPath = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "/" : "C:\\";
    private static readonly string BudsPath = Path.Combine(RootPath, "Buds");
    private static readonly string ConfigPath = Path.Combine(RootPath, "Config");
    private static readonly string LoaderPath = Path.Combine(RootPath, nameof(VenusRootLoader));

    private readonly IBudsDiscoverer _budsDiscoverer = Substitute.For<IBudsDiscoverer>();
    private readonly IBudsValidator _budsValidator = Substitute.For<IBudsValidator>();
    private readonly IBudsDependencySorter _budsDependencySorter = Substitute.For<IBudsDependencySorter>();
    private readonly IBudsLoadOrderEnumerator _budsLoadOrderEnumerator = Substitute.For<IBudsLoadOrderEnumerator>();
    private readonly IAssemblyLoader _assemblyLoader = Substitute.For<IAssemblyLoader>();
    private readonly ILoggerFactory _loggerFactory = Substitute.For<ILoggerFactory>();
    private readonly IVenusFactory _venusFactory = Substitute.For<IVenusFactory>();
    private readonly IBudConfigManager _budConfigManager = Substitute.For<IBudConfigManager>();
    private readonly FakeLogger<BudLoader> _logger = new();
    private readonly MockFileSystem _fileSystem = new();

    private readonly BudLoaderContext _budLoaderContext = new()
    {
        BudsPath = BudsPath,
        ConfigPath = ConfigPath,
        LoaderPath = LoaderPath
    };

    private readonly BudLoader _sut;

    public BudLoaderTests()
    {
        _fileSystem.Directory.CreateDirectory(ConfigPath);
        _fileSystem.Directory.CreateDirectory(LoaderPath);

        _sut = new(
            _budsDiscoverer,
            _budsValidator,
            _budsDependencySorter,
            _budsLoadOrderEnumerator,
            _assemblyLoader,
            _budLoaderContext,
            _fileSystem,
            _logger,
            _loggerFactory,
            _venusFactory,
            _budConfigManager);
    }

    [Fact]
    public void LoadAllBuds_DoesNothing_WhenDiscovererFindsNoBuds()
    {
        _budsDiscoverer.DiscoverAllBudsFromDisk().ReturnsForAnyArgs([]);

        _sut.LoadAllBuds();

        _logger.Collector.GetSnapshot().Should().BeEmpty();

        _budsDiscoverer.Received(1).DiscoverAllBudsFromDisk();
        _budsValidator.DidNotReceiveWithAnyArgs().RemoveInvalidBuds(null!);
        _budsDependencySorter.DidNotReceiveWithAnyArgs().SortBudsTopologicallyFromDependencyGraph(null!);
        _budsLoadOrderEnumerator.DidNotReceiveWithAnyArgs().EnumerateBudsWithFulfilledDependencies(null!);
        _budsLoadOrderEnumerator.DidNotReceiveWithAnyArgs().MarkBudAsFailedDuringLoad(null!);
        _assemblyLoader.DidNotReceiveWithAnyArgs().LoadFromPath(null!);
        _loggerFactory.DidNotReceiveWithAnyArgs().CreateLogger(null!);
        _venusFactory.DidNotReceiveWithAnyArgs().CreateVenusForBud(null!);
        _budConfigManager.DidNotReceiveWithAnyArgs().GetConfigPathForBud(null!);
        _budConfigManager.DidNotReceiveWithAnyArgs().Save(null!, null!, null!, null!);
        _budConfigManager.DidNotReceiveWithAnyArgs().Load(null!, null!);
    }

    [Fact]
    public void LoadAllBuds_DoesNothingAndLogsError_WhenDiscovererThrowsAnException()
    {
        Exception exception = new("An exception");
        _budsDiscoverer.DiscoverAllBudsFromDisk().ThrowsForAnyArgs(exception);

        _sut.LoadAllBuds();

        TestUtility.AssertErrorLogs(_logger, 1, exception.Message);

        _budsDiscoverer.Received(1).DiscoverAllBudsFromDisk();
        _budsValidator.DidNotReceiveWithAnyArgs().RemoveInvalidBuds(null!);
        _budsDependencySorter.DidNotReceiveWithAnyArgs().SortBudsTopologicallyFromDependencyGraph(null!);
        _budsLoadOrderEnumerator.DidNotReceiveWithAnyArgs().EnumerateBudsWithFulfilledDependencies(null!);
        _budsLoadOrderEnumerator.DidNotReceiveWithAnyArgs().MarkBudAsFailedDuringLoad(null!);
        _assemblyLoader.DidNotReceiveWithAnyArgs().LoadFromPath(null!);
        _loggerFactory.DidNotReceiveWithAnyArgs().CreateLogger(null!);
        _venusFactory.DidNotReceiveWithAnyArgs().CreateVenusForBud(null!);
        _budConfigManager.DidNotReceiveWithAnyArgs().GetConfigPathForBud(null!);
        _budConfigManager.DidNotReceiveWithAnyArgs().Save(null!, null!, null!, null!);
        _budConfigManager.DidNotReceiveWithAnyArgs().Load(null!, null!);
    }

    [Fact]
    public void LoadAllBuds_DoesNothingAndLogsError_WhenDependencySorterThrowsAnException()
    {
        Exception exception = new("An exception");
        string budId = "aBudId";
        _fileSystem.Directory.CreateDirectory(Path.Combine(BudsPath, budId));
        MethodInfo mainMethod = typeof(BudLoaderTests).GetMethod(
            nameof(MainExceptionThrow),
            BindingFlags.Static | BindingFlags.NonPublic)!;
        BudInfo[] buds = [CreateBud(budId, mainMethod)];

        _budsDiscoverer.DiscoverAllBudsFromDisk().ReturnsForAnyArgs(buds);
        Dictionary<string, BudInfo> validatedBuds = buds.ToDictionary(bud => bud.BudManifest.BudId, bud => bud);
        _budsValidator.RemoveInvalidBuds(buds).ReturnsForAnyArgs(validatedBuds);
        _budsDependencySorter.SortBudsTopologicallyFromDependencyGraph(null!).ThrowsForAnyArgs(exception);

        _sut.LoadAllBuds();

        TestUtility.AssertErrorLogs(_logger, 1, exception.Message);

        _budsDiscoverer.Received(1).DiscoverAllBudsFromDisk();
        _budsValidator.Received(1).RemoveInvalidBuds(buds);
        _budsDependencySorter.Received(1).SortBudsTopologicallyFromDependencyGraph(validatedBuds);
        _budsLoadOrderEnumerator.DidNotReceiveWithAnyArgs().EnumerateBudsWithFulfilledDependencies(null!);
        _budsLoadOrderEnumerator.DidNotReceiveWithAnyArgs().MarkBudAsFailedDuringLoad(null!);
        _assemblyLoader.DidNotReceiveWithAnyArgs().LoadFromPath(null!);
        _loggerFactory.DidNotReceiveWithAnyArgs().CreateLogger(null!);
        _venusFactory.DidNotReceiveWithAnyArgs().CreateVenusForBud(null!);
        _budConfigManager.DidNotReceiveWithAnyArgs().GetConfigPathForBud(null!);
        _budConfigManager.DidNotReceiveWithAnyArgs().Save(null!, null!, null!, null!);
        _budConfigManager.DidNotReceiveWithAnyArgs().Load(null!, null!);
    }

    [Fact]
    public void LoadAllBuds_MarkBudAsFailed_WhenAssemblyLoaderThrowsAnException()
    {
        Exception exception = new("An exception");
        string budId = "aBudId";
        MethodInfo mainMethod = typeof(BudLoaderTests).GetMethod(
            nameof(MainExceptionThrow),
            BindingFlags.Static | BindingFlags.NonPublic)!;
        BudInfo[] buds = [CreateBud(budId, mainMethod)];

        _budsDiscoverer.DiscoverAllBudsFromDisk().ReturnsForAnyArgs(buds);
        Dictionary<string, BudInfo> validBuds = buds.ToDictionary(bud => bud.BudManifest.BudId, bud => bud);
        _budsValidator.RemoveInvalidBuds(buds).ReturnsForAnyArgs(validBuds);
        _budsDependencySorter.SortBudsTopologicallyFromDependencyGraph(null!).ReturnsForAnyArgs(buds);
        _budsLoadOrderEnumerator.EnumerateBudsWithFulfilledDependencies(null!).ReturnsForAnyArgs(buds);
        _assemblyLoader.LoadFromPath(null!).ThrowsForAnyArgs(exception);

        _sut.LoadAllBuds();

        TestUtility.AssertErrorLogs(_logger, 1, exception.Message);

        _budsDiscoverer.Received(1).DiscoverAllBudsFromDisk();
        _budsValidator.Received(1).RemoveInvalidBuds(buds);
        _budsDependencySorter.Received(1).SortBudsTopologicallyFromDependencyGraph(validBuds);
        _budsLoadOrderEnumerator.Received(1).EnumerateBudsWithFulfilledDependencies(buds);
        _budsLoadOrderEnumerator.Received(1).MarkBudAsFailedDuringLoad(buds[0]);
        _assemblyLoader.Received(1).LoadFromPath(Path.Combine(BudsPath, budId, $"{budId}.dll"));
        _loggerFactory.DidNotReceiveWithAnyArgs().CreateLogger(null!);
        _venusFactory.DidNotReceiveWithAnyArgs().CreateVenusForBud(null!);
        _budConfigManager.DidNotReceiveWithAnyArgs().GetConfigPathForBud(null!);
        _budConfigManager.DidNotReceiveWithAnyArgs().Save(null!, null!, null!, null!);
        _budConfigManager.DidNotReceiveWithAnyArgs().Load(null!, null!);
    }

    private static readonly Exception TestException = new("An exception");
    internal static void MainExceptionThrow() => throw TestException;

    [Fact]
    public void LoadAllBuds_MarkBudAsFailed_WhenBudMainThrowsAnException()
    {
        string budId = "aBudId";
        MethodInfo exceptionThrowMethod = typeof(BudLoaderTests).GetMethod(
            nameof(MainExceptionThrow),
            BindingFlags.Static | BindingFlags.NonPublic)!;
        BudInfo[] buds = [CreateBud(budId, exceptionThrowMethod)];

        _budsDiscoverer.DiscoverAllBudsFromDisk().ReturnsForAnyArgs(buds);
        Dictionary<string, BudInfo> validBuds = buds.ToDictionary(bud => bud.BudManifest.BudId, bud => bud);
        _budsValidator.RemoveInvalidBuds(buds).ReturnsForAnyArgs(validBuds);
        _budsDependencySorter.SortBudsTopologicallyFromDependencyGraph(null!).ReturnsForAnyArgs(buds);
        _budsLoadOrderEnumerator.EnumerateBudsWithFulfilledDependencies(null!).ReturnsForAnyArgs(buds);
        _assemblyLoader.LoadFromPath(null!).ReturnsForAnyArgs(x =>
        {
            byte[] assemblyBytes = _fileSystem.File.ReadAllBytes((string)x[0]);
            return Assembly.Load(assemblyBytes);
        });

        _sut.LoadAllBuds();

        TestUtility.AssertErrorLogs(_logger, 1, TestException.Message);

        _budsDiscoverer.Received(1).DiscoverAllBudsFromDisk();
        _budsValidator.Received(1).RemoveInvalidBuds(buds);
        _budsDependencySorter.Received(1).SortBudsTopologicallyFromDependencyGraph(validBuds);
        _budsLoadOrderEnumerator.Received(1).EnumerateBudsWithFulfilledDependencies(buds);
        _budsLoadOrderEnumerator.Received(1).MarkBudAsFailedDuringLoad(buds[0]);
        _assemblyLoader.Received(1).LoadFromPath(Path.Combine(BudsPath, budId, $"{budId}.dll"));
        _loggerFactory.Received(1).CreateLogger(budId);
        _venusFactory.Received(1).CreateVenusForBud(budId);
        _budConfigManager.DidNotReceiveWithAnyArgs().GetConfigPathForBud(null!);
        _budConfigManager.DidNotReceiveWithAnyArgs().Save(null!, null!, null!, null!);
        _budConfigManager.DidNotReceiveWithAnyArgs().Load(null!, null!);
    }

    private static bool _mainCalledAndReturned;
    internal static void MainNormal() => _mainCalledAndReturned = true;

    [Fact]
    public void LoadAllBuds_LoadBud_WhenBudLoadsSuccessfully()
    {
        string budId = "aBudId";
        MethodInfo exceptionThrowMethod = typeof(BudLoaderTests).GetMethod(
            nameof(MainNormal),
            BindingFlags.Static | BindingFlags.NonPublic)!;
        BudInfo[] buds = [CreateBud(budId, exceptionThrowMethod)];

        _budsDiscoverer.DiscoverAllBudsFromDisk().ReturnsForAnyArgs(buds);
        Dictionary<string, BudInfo> validBuds = buds.ToDictionary(bud => bud.BudManifest.BudId, bud => bud);
        _budsValidator.RemoveInvalidBuds(buds).ReturnsForAnyArgs(validBuds);
        _budsDependencySorter.SortBudsTopologicallyFromDependencyGraph(null!).ReturnsForAnyArgs(buds);
        _budsLoadOrderEnumerator.EnumerateBudsWithFulfilledDependencies(null!).ReturnsForAnyArgs(buds);
        _assemblyLoader.LoadFromPath(null!).ReturnsForAnyArgs(x =>
        {
            byte[] assemblyBytes = _fileSystem.File.ReadAllBytes((string)x[0]);
            return Assembly.Load(assemblyBytes);
        });
        _mainCalledAndReturned = false;

        _sut.LoadAllBuds();

        TestUtility.AssertErrorLogs(_logger, 0);
        _mainCalledAndReturned.Should().BeTrue();

        _budsDiscoverer.Received(1).DiscoverAllBudsFromDisk();
        _budsValidator.Received(1).RemoveInvalidBuds(buds);
        _budsDependencySorter.Received(1).SortBudsTopologicallyFromDependencyGraph(validBuds);
        _budsLoadOrderEnumerator.Received(1).EnumerateBudsWithFulfilledDependencies(buds);
        _budsLoadOrderEnumerator.DidNotReceiveWithAnyArgs().MarkBudAsFailedDuringLoad(null!);
        _assemblyLoader.Received(1).LoadFromPath(Path.Combine(BudsPath, budId, $"{budId}.dll"));
        _loggerFactory.Received(1).CreateLogger(budId);
        _venusFactory.Received(1).CreateVenusForBud(budId);
        _budConfigManager.DidNotReceiveWithAnyArgs().GetConfigPathForBud(null!);
        _budConfigManager.DidNotReceiveWithAnyArgs().Save(null!, null!, null!, null!);
        _budConfigManager.DidNotReceiveWithAnyArgs().Load(null!, null!);
    }

    private sealed class DefaultConfigTest
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public int SomeIntValue { get; set; }
    }

    internal static object DefaultConfigGetter() => new DefaultConfigTest { SomeIntValue = 5 };

    [Fact]
    public void LoadAllBuds_LoadBudAndSaveDefaultConfig_WhenBudLoadSuccessfullyAndHasDefaultConfigData()
    {
        const string budId = "aBudId";
        MethodInfo exceptionThrowMethod = typeof(BudLoaderTests).GetMethod(
            nameof(MainNormal),
            BindingFlags.Static | BindingFlags.NonPublic)!;
        MethodInfo defaultConfigGetter = typeof(BudLoaderTests).GetMethod(
            nameof(DefaultConfigGetter),
            BindingFlags.Static | BindingFlags.NonPublic)!;
        BudInfo[] buds = [CreateBud(budId, exceptionThrowMethod, defaultConfigGetter)];

        _budsDiscoverer.DiscoverAllBudsFromDisk().ReturnsForAnyArgs(buds);
        Dictionary<string, BudInfo> validBuds = buds.ToDictionary(bud => bud.BudManifest.BudId, bud => bud);
        _budsValidator.RemoveInvalidBuds(buds).ReturnsForAnyArgs(validBuds);
        _budsDependencySorter.SortBudsTopologicallyFromDependencyGraph(null!).ReturnsForAnyArgs(buds);
        _budsLoadOrderEnumerator.EnumerateBudsWithFulfilledDependencies(null!).ReturnsForAnyArgs(buds);
        _assemblyLoader.LoadFromPath(null!).ReturnsForAnyArgs(x =>
        {
            byte[] assemblyBytes = _fileSystem.File.ReadAllBytes((string)x[0]);
            return Assembly.Load(assemblyBytes);
        });
        _budConfigManager.GetConfigPathForBud(null!).ReturnsForAnyArgs(x => Path.Combine(ConfigPath, $"{x[0]}.toml"));
        _mainCalledAndReturned = false;

        _sut.LoadAllBuds();

        TestUtility.AssertErrorLogs(_logger, 0);
        _mainCalledAndReturned.Should().BeTrue();

        _budsDiscoverer.Received(1).DiscoverAllBudsFromDisk();
        _budsValidator.Received(1).RemoveInvalidBuds(buds);
        _budsDependencySorter.Received(1).SortBudsTopologicallyFromDependencyGraph(validBuds);
        _budsLoadOrderEnumerator.Received(1).EnumerateBudsWithFulfilledDependencies(buds);
        _budsLoadOrderEnumerator.DidNotReceiveWithAnyArgs().MarkBudAsFailedDuringLoad(null!);
        _assemblyLoader.Received(1).LoadFromPath(Path.Combine(BudsPath, budId, $"{budId}.dll"));
        _loggerFactory.Received(1).CreateLogger(budId);
        _venusFactory.Received(1).CreateVenusForBud(budId);
        _budConfigManager.Received(1).GetConfigPathForBud(budId);
        _budConfigManager.Received(1).Save(budId, typeof(DefaultConfigTest), Arg.Any<object>(), Arg.Any<object>());
        _budConfigManager.DidNotReceiveWithAnyArgs().Load(null!, null!);
    }

    [Fact]
    public void LoadAllBuds_LoadBudAndLoadConfigFile_WhenBudLoadSuccessfullyAndHasConfigFile()
    {
        const string budId = "aBudId";
        MethodInfo exceptionThrowMethod = typeof(BudLoaderTests).GetMethod(
            nameof(MainNormal),
            BindingFlags.Static | BindingFlags.NonPublic)!;
        MethodInfo defaultConfigGetter = typeof(BudLoaderTests).GetMethod(
            nameof(DefaultConfigGetter),
            BindingFlags.Static | BindingFlags.NonPublic)!;
        BudInfo[] buds = [CreateBud(budId, exceptionThrowMethod, defaultConfigGetter)];

        _budsDiscoverer.DiscoverAllBudsFromDisk().ReturnsForAnyArgs(buds);
        Dictionary<string, BudInfo> validBuds = buds.ToDictionary(bud => bud.BudManifest.BudId, bud => bud);
        _budsValidator.RemoveInvalidBuds(buds).ReturnsForAnyArgs(validBuds);
        _budsDependencySorter.SortBudsTopologicallyFromDependencyGraph(null!).ReturnsForAnyArgs(buds);
        _budsLoadOrderEnumerator.EnumerateBudsWithFulfilledDependencies(null!).ReturnsForAnyArgs(buds);
        _assemblyLoader.LoadFromPath(null!).ReturnsForAnyArgs(x =>
        {
            byte[] assemblyBytes = _fileSystem.File.ReadAllBytes((string)x[0]);
            return Assembly.Load(assemblyBytes);
        });
        _budConfigManager.GetConfigPathForBud(null!).ReturnsForAnyArgs(x => Path.Combine(ConfigPath, $"{x[0]}.toml"));
        _fileSystem.File.CreateText(Path.Combine(ConfigPath, $"{budId}.toml")).Close();
        _mainCalledAndReturned = false;

        _sut.LoadAllBuds();

        TestUtility.AssertErrorLogs(_logger, 0);
        _mainCalledAndReturned.Should().BeTrue();

        _budsDiscoverer.Received(1).DiscoverAllBudsFromDisk();
        _budsValidator.Received(1).RemoveInvalidBuds(buds);
        _budsDependencySorter.Received(1).SortBudsTopologicallyFromDependencyGraph(validBuds);
        _budsLoadOrderEnumerator.Received(1).EnumerateBudsWithFulfilledDependencies(buds);
        _budsLoadOrderEnumerator.DidNotReceiveWithAnyArgs().MarkBudAsFailedDuringLoad(null!);
        _assemblyLoader.Received(1).LoadFromPath(Path.Combine(BudsPath, budId, $"{budId}.dll"));
        _loggerFactory.Received(1).CreateLogger(budId);
        _venusFactory.Received(1).CreateVenusForBud(budId);
        _budConfigManager.Received(1).GetConfigPathForBud(budId);
        _budConfigManager.Received(1).Save(budId, typeof(DefaultConfigTest), Arg.Any<object>(), Arg.Any<object>());
        _budConfigManager.Received(1).Load(budId, typeof(DefaultConfigTest));
    }

    private BudInfo CreateBud(string budId, MethodInfo mainMethod, MethodInfo? defaultConfigGetter = null)
    {
        _fileSystem.Directory.CreateDirectory(Path.Combine(BudsPath, budId));
        string assemblyName = $"{budId}.dll";

        DotNetRuntimeInfo net472RuntimeInfo = DotNetRuntimeInfo.Parse(".NETFramework,Version=v4.7.2");
        AssemblyDefinition assemblyDefinition = new(new(assemblyName), new Version(1, 0, 0));
        ModuleDefinition budModuleDefinition = new(new(budId), net472RuntimeInfo.GetDefaultCorLib());

        PermissionSetSignature permissionSetSignature = new();
        ITypeDefOrRef securityPermissionAttribute =
            budModuleDefinition.DefaultImporter.ImportType(typeof(SecurityPermissionAttribute));
        SecurityAttribute securityAttribute = new(securityPermissionAttribute.ToTypeSignature());
        CustomAttributeNamedArgument customAttributeNamedArgument = new(
            CustomAttributeArgumentMemberType.Property,
            new(nameof(SecurityPermissionAttribute.SkipVerification)),
            budModuleDefinition.CorLibTypeFactory.Boolean,
            new(budModuleDefinition.CorLibTypeFactory.Boolean, true));
        securityAttribute.NamedArguments.Add(customAttributeNamedArgument);
        permissionSetSignature.Attributes.Add(securityAttribute);
        SecurityDeclaration securityDeclaration = new(SecurityAction.RequestMinimum, permissionSetSignature);
        assemblyDefinition.SecurityDeclarations.Add(securityDeclaration);

        ITypeDefOrRef baseBudTypeReference = budModuleDefinition.DefaultImporter.ImportType(typeof(Bud));
        TypeDefinition budType = new("namespace", $"{budId}budType", TypeAttributes.Class, baseBudTypeReference);
        MethodDefinition budTypeCtor = MethodDefinition.CreateConstructor(budModuleDefinition);
        budType.Methods.Add(budTypeCtor);

        MethodDefinition budTypeMain = new(
            "Main",
            MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig |
            MethodAttributes.ReuseSlot,
            MethodSignature.CreateInstance(budModuleDefinition.CorLibTypeFactory.Void));
        CilMethodBody mainBody = new();
        ReferenceImporter importer = new(budModuleDefinition);
        IMethodDescriptor testMethod = importer.ImportMethod(mainMethod);
        mainBody.Instructions.Add(CilOpCodes.Call, testMethod);
        mainBody.Instructions.Add(CilOpCodes.Ret);
        budTypeMain.CilMethodBody = mainBody;
        budType.Methods.Add(budTypeMain);

        if (defaultConfigGetter is not null)
        {
            MethodDefinition budTypeDefaultConfigDataGetter = new(
                new($"get_{nameof(Bud.DefaultConfigData)}"),
                MethodAttributes.Family | MethodAttributes.Virtual | MethodAttributes.HideBySig |
                MethodAttributes.ReuseSlot | MethodAttributes.SpecialName,
                MethodSignature.CreateInstance(budModuleDefinition.CorLibTypeFactory.Object));
            CilMethodBody configDataGetterBody = new();
            IMethodDescriptor defaultConfigGetterMethod = importer.ImportMethod(defaultConfigGetter);
            configDataGetterBody.Instructions.Add(CilOpCodes.Call, defaultConfigGetterMethod);

            configDataGetterBody.Instructions.Add(CilOpCodes.Ret);
            budTypeDefaultConfigDataGetter.CilMethodBody = configDataGetterBody;
            budType.Methods.Add(budTypeDefaultConfigDataGetter);
        }

        budModuleDefinition.TopLevelTypes.Add(budType);
        assemblyDefinition.Modules.Add(budModuleDefinition);

        using FileSystemStream assemblyStream = _fileSystem.File.Create(Path.Combine(BudsPath, budId, assemblyName));
        assemblyDefinition.WriteManifest(assemblyStream);
        assemblyDefinition.Write("/home/aldelaro5/assembly.dll");

        BudInfo bud = new()
        {
            BudManifest = new()
            {
                AssemblyName = assemblyName,
                BudId = budId,
                BudName = $"{budId} name",
                BudVersion = new(1, 0, 0),
                BudAuthor = $"{budId} author",
                BudDependencies = [],
                BudIncompatibilities = []
            },
            BudAssemblyPath = Path.Combine(BudsPath, budId, assemblyName),
            BudType = budType
        };
        return bud;
    }
}