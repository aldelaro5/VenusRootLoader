using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using System.Runtime.InteropServices;
using VenusRootLoader.Api;
using VenusRootLoader.BudLoading;

namespace VenusRootLoader.Tests;

public sealed class AppDomainEventsHandlerTests
{
    private static readonly string RootPath = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "/" : "C:\\";
    private static readonly string BudsPath = Path.Combine(RootPath, "Buds");
    private static readonly string ConfigPath = Path.Combine(RootPath, "Config");
    private static readonly string LoaderPath = Path.Combine(RootPath, nameof(VenusRootLoader));

    private readonly IAssemblyLoader _assemblyLoader = Substitute.For<IAssemblyLoader>();
    private readonly IAppDomainEvents _appDomainEvents = Substitute.For<IAppDomainEvents>();

    private readonly FakeLogger<AppDomainEventsHandler> _logger = new();

    private readonly MockFileSystem _fileSystem = new();

    private readonly BudLoaderContext _budLoaderContext = new()
    {
        BudsPath = BudsPath,
        ConfigPath = ConfigPath,
        LoaderPath = LoaderPath
    };

    private readonly AppDomainEventsHandler _sut;

    public AppDomainEventsHandlerTests()
    {
        _fileSystem.Directory.CreateDirectory(BudsPath);
        _fileSystem.Directory.CreateDirectory(ConfigPath);
        _fileSystem.Directory.CreateDirectory(LoaderPath);

        _sut = new(
            _budLoaderContext,
            _assemblyLoader,
            _appDomainEvents,
            _logger,
            _fileSystem);
    }

    [Fact]
    public void InstallHandlers_SubscribeToAppDomainEvents_WhenCalled()
    {
        _sut.InstallHandlers();

        _appDomainEvents.Received(1).AssemblyResolve += Arg.Any<ResolveEventHandler>();
        _appDomainEvents.Received(1).UnhandledException +=
            Arg.Any<UnhandledExceptionEventHandler>();
    }

    [Fact]
    public void UnhandledException_LogsCritical_WhenReceivedException()
    {
        _sut.InstallHandlers();

        Exception exception = new("Some exception message");
        _appDomainEvents.UnhandledException += Raise.Event<UnhandledExceptionEventHandler>(
            this,
            new UnhandledExceptionEventArgs(exception, false));

        _logger.LatestRecord.Level.Should().Be(LogLevel.Critical);
        _logger.LatestRecord.Exception.Should().Be(exception);
    }

    [Fact]
    public void UnhandledException_LogsCritical_WhenReceivedExceptionObject()
    {
        _sut.InstallHandlers();

        string exceptionObject = "Some exception message";
        _appDomainEvents.UnhandledException += Raise.Event<UnhandledExceptionEventHandler>(
            this,
            new UnhandledExceptionEventArgs(exceptionObject, false));

        _logger.LatestRecord.Level.Should().Be(LogLevel.Critical);
        _logger.LatestRecord.Message.Should().Contain(exceptionObject);
    }

    [Fact]
    public void AssemblyResolve_LogsWarning_WhenAssemblyDoesNotExist()
    {
        _sut.InstallHandlers();

        const string assemblyName = "AnAssembly";
        _fileSystem.File.Create(Path.Combine(BudsPath, "UnexpectedAssembly.dll")).Dispose();
        _fileSystem.File.Create(Path.Combine(BudsPath, "UnexpectedAssembly.exe")).Dispose();
        _fileSystem.File.Create(Path.Combine(LoaderPath, "UnexpectedAssembly.dll")).Dispose();
        _fileSystem.File.Create(Path.Combine(LoaderPath, "UnexpectedAssembly.exe")).Dispose();
        _appDomainEvents.AssemblyResolve += Raise.Event<ResolveEventHandler>(
            this,
            new ResolveEventArgs(assemblyName));

        _assemblyLoader.DidNotReceiveWithAnyArgs().LoadFromPath("");
        _logger.LatestRecord.Level.Should().Be(LogLevel.Warning);
        _logger.LatestRecord.Message.Should().Contain(assemblyName);
    }

    public static IEnumerable<object[]> AssemblyResolveTestData()
    {
        yield return [BudsPath, ".exe"];
        yield return [Path.Combine(BudsPath, "nested"), ".exe"];
        yield return [BudsPath, ".dll"];
        yield return [Path.Combine(BudsPath, "nested"), ".dll"];
        yield return [LoaderPath, ".dll"];
    }
    
    [Theory]
    [MemberData(nameof(AssemblyResolveTestData))]
    public void AssemblyResolve_LoadsAssembly_WhenAssemblyExists(string basePath, string extension)
    {
        _sut.InstallHandlers();

        const string assemblyName = "AnAssembly";
        _fileSystem.Directory.CreateDirectory(basePath);
        _fileSystem.File.Create(Path.Combine(basePath, $"AnAssembly{extension}")).Dispose();
        _fileSystem.File.Create(Path.Combine(LoaderPath, "UnexpectedAssembly.dll")).Dispose();
        _fileSystem.File.Create(Path.Combine(LoaderPath, "UnexpectedAssembly.exe")).Dispose();
        _fileSystem.File.Create(Path.Combine(BudsPath, "UnexpectedAssembly.dll")).Dispose();
        _fileSystem.File.Create(Path.Combine(BudsPath, "UnexpectedAssembly.exe")).Dispose();
        _appDomainEvents.AssemblyResolve += Raise.Event<ResolveEventHandler>(
            this,
            new ResolveEventArgs(assemblyName));

        _assemblyLoader.Received(1).LoadFromPath(Path.Combine(basePath, $"{assemblyName}{extension}"));
    }
}