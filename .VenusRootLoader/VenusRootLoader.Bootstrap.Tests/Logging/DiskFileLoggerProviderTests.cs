using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using System.IO.Abstractions.TestingHelpers;
using VenusRootLoader.Bootstrap.Logging;
using VenusRootLoader.Bootstrap.Settings.LogProvider;
using VenusRootLoader.Bootstrap.Shared;

namespace VenusRootLoader.Bootstrap.Tests.Logging;

public sealed class DiskFileLoggerProviderTests
{
    private readonly IOptions<DiskFileLoggerSettings> _diskFileLoggerSettings =
        Substitute.For<IOptions<DiskFileLoggerSettings>>();

    private readonly FakeTimeProvider _timeProvider = new();
    private readonly IBootstrapEnvironment _bootstrapEnvironment = Substitute.For<IBootstrapEnvironment>();
    private readonly MockFileSystem _fileSystem = new();

    [Fact]
    public void CreateLogger_ReturnsNullLogger_WhenDiskFileLoggingIsDisabled()
    {
        _diskFileLoggerSettings.Value.Returns(
            new DiskFileLoggerSettings
            {
                Enable = false,
                MaxFilesToKeep = 5
            });

        DiskFileLoggerProvider sut = new DiskFileLoggerProvider(
            _diskFileLoggerSettings,
            _bootstrapEnvironment,
            _fileSystem,
            _timeProvider);
        ILogger logger = sut.CreateLogger("Test");

        logger.Should().BeOfType<NullLogger>();
    }

    [Fact]
    public void CreateLogger_ReturnsNullLogger_WhenLogFileIsAlreadyOpened()
    {
        string rootPath = Path.Combine(Directory.GetDirectoryRoot(Directory.GetCurrentDirectory()), "root");
        string existingLogPath = Path.Combine(
            rootPath,
            "Logs",
            "latest.log");
        string existingLogsContent = "existing logs";

        _diskFileLoggerSettings.Value.Returns(
            new DiskFileLoggerSettings
            {
                Enable = true,
                MaxFilesToKeep = 5
            });
        _bootstrapEnvironment.BasePath.Returns(rootPath);
        _fileSystem.AddFile(existingLogPath, new(existingLogsContent) { AllowedFileShare = FileShare.None });

        DiskFileLoggerProvider sut = new DiskFileLoggerProvider(
            _diskFileLoggerSettings,
            _bootstrapEnvironment,
            _fileSystem,
            _timeProvider);
        ILogger logger = sut.CreateLogger("Test");

        logger.Should().BeOfType<NullLogger>();
        _fileSystem.AllFiles.Should().HaveCount(1);
        _fileSystem.AllFiles.ElementAt(0).Should().Be(existingLogPath);
    }

    [Fact]
    public void CreateLogger_ReturnsDiskFileLoggerWithCorrectFilename_WhenDiskFileLoggingIsEnabled()
    {
        string rootPath = Path.Combine(Directory.GetDirectoryRoot(Directory.GetCurrentDirectory()), "root");
        string expectedPath = Path.Combine(
            rootPath,
            "Logs",
            "latest.log");

        _diskFileLoggerSettings.Value.Returns(
            new DiskFileLoggerSettings
            {
                Enable = true,
                MaxFilesToKeep = 5
            });
        _bootstrapEnvironment.BasePath.Returns(rootPath);

        using DiskFileLoggerProvider sut = new DiskFileLoggerProvider(
            _diskFileLoggerSettings,
            _bootstrapEnvironment,
            _fileSystem,
            _timeProvider);
        ILogger logger = sut.CreateLogger("Test");

        logger.Should().BeOfType<DiskFileLogger>();
        _fileSystem.AllFiles.Should().ContainSingle(expectedPath);
    }

    [Fact]
    public void CreateLogger_ReturnsDiskFileLoggerAfterOrganisingLogFiles_WhenALogFileExistsAlready()
    {
        string rootPath = Path.Combine(Directory.GetDirectoryRoot(Directory.GetCurrentDirectory()), "root");
        string latestLogPath = Path.Combine(
            rootPath,
            "Logs",
            "latest.log");
        string existingLogsContent = "existing logs";
        DateTime olderLogFileTimeStamp = new DateTime(2025, 6, 15, 12, 30, 30);
        string olderLogPath = Path.Combine(
            rootPath,
            "Logs",
            $"{olderLogFileTimeStamp:yyyy-MM-dd_HH-mm-ss}.log");
        DateTimeOffset currentTime = DateTimeOffset.Now;

        _diskFileLoggerSettings.Value.Returns(
            new DiskFileLoggerSettings
            {
                Enable = true,
                MaxFilesToKeep = 5
            });
        _bootstrapEnvironment.BasePath.Returns(rootPath);
        _fileSystem.AddFile(latestLogPath, new(existingLogsContent));
        _fileSystem.File.SetCreationTime(latestLogPath, olderLogFileTimeStamp);

        _timeProvider.SetLocalTimeZone(TimeZoneInfo.Utc);
        _timeProvider.SetUtcNow(currentTime);

        using DiskFileLoggerProvider sut = new DiskFileLoggerProvider(
            _diskFileLoggerSettings,
            _bootstrapEnvironment,
            _fileSystem,
            _timeProvider);
        ILogger logger = sut.CreateLogger("Test");

        logger.Should().BeOfType<DiskFileLogger>();
        _fileSystem.AllFiles.Should().ContainSingle(p => p == olderLogPath);
        _fileSystem.GetFile(olderLogPath).Should().Satisfy<MockFileData>(f =>
        {
            f.TextContents.Should().Be(existingLogsContent);
            f.CreationTime.Should().Be(olderLogFileTimeStamp);
        });
        _fileSystem.AllFiles.Should().ContainSingle(p => p == latestLogPath);
        _fileSystem.GetFile(latestLogPath).Should().Satisfy<MockFileData>(f =>
        {
            f.TextContents.Should().Be(string.Empty);
            f.CreationTime.Should().Be(currentTime);
        });
    }

    [Fact]
    public void CreateLogger_ReturnsDiskFileLoggerAfterDeletingOldFiles_WhenTheAmountOfFilesExceedsTheLimit()
    {
        string rootPath = Path.Combine(Directory.GetDirectoryRoot(Directory.GetCurrentDirectory()), "root");
        string latestLogPath = Path.Combine(
            rootPath,
            "Logs",
            "latest.log");
        string newerLogContent = "newer logs";
        string olderLogContent = "older logs";
        DateTime newerLogTimeStamp = new DateTime(2025, 6, 15, 12, 30, 30);
        string newerLogPath = Path.Combine(
            rootPath,
            "Logs",
            $"{newerLogTimeStamp:yyyy-MM-dd_HH-mm-ss}.log");
        DateTimeOffset currentTime = DateTimeOffset.Now;

        _diskFileLoggerSettings.Value.Returns(
            new DiskFileLoggerSettings
            {
                Enable = true,
                MaxFilesToKeep = 2
            });
        _bootstrapEnvironment.BasePath.Returns(rootPath);
        _fileSystem.AddFile(latestLogPath, new(newerLogContent));
        _fileSystem.AddFile(newerLogPath, new(olderLogContent));
        _fileSystem.File.SetCreationTime(latestLogPath, newerLogTimeStamp);
        _fileSystem.File.SetCreationTime(newerLogPath, newerLogTimeStamp.AddDays(-1));

        _timeProvider.SetLocalTimeZone(TimeZoneInfo.Utc);
        _timeProvider.SetUtcNow(currentTime);

        using DiskFileLoggerProvider sut = new DiskFileLoggerProvider(
            _diskFileLoggerSettings,
            _bootstrapEnvironment,
            _fileSystem,
            _timeProvider);
        ILogger logger = sut.CreateLogger("Test");

        logger.Should().BeOfType<DiskFileLogger>();
        _fileSystem.AllFiles.Should().HaveCount(2);
        _fileSystem.AllFiles.Should().ContainSingle(p => p == newerLogPath);
        _fileSystem.GetFile(newerLogPath).Should().Satisfy<MockFileData>(f =>
        {
            f.TextContents.Should().Be(newerLogContent);
            f.CreationTime.Should().Be(newerLogTimeStamp);
        });
        _fileSystem.AllFiles.Should().ContainSingle(p => p == latestLogPath);
        _fileSystem.GetFile(latestLogPath).Should().Satisfy<MockFileData>(f =>
        {
            f.TextContents.Should().Be(string.Empty);
            f.CreationTime.Should().Be(currentTime);
        });
    }
}