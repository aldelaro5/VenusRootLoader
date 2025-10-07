using AwesomeAssertions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using System.IO.Abstractions.TestingHelpers;
using VenusRootLoader.Bootstrap.Logging;
using VenusRootLoader.Bootstrap.Settings.LogProvider;

namespace VenusRootLoader.Bootstrap.Tests.Logging;

public sealed class DiskFileLoggerProviderTests
{
    private readonly IOptions<DiskFileLoggerSettings> _diskFileLoggerSettings =
        Substitute.For<IOptions<DiskFileLoggerSettings>>();

    private readonly FakeTimeProvider _timeProvider = new();
    private readonly IHostEnvironment _hostEnvironment = Substitute.For<IHostEnvironment>();
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

        var sut = new DiskFileLoggerProvider(_diskFileLoggerSettings, _hostEnvironment, _fileSystem, _timeProvider);
        var logger = sut.CreateLogger("Test");

        logger.Should().BeOfType<NullLogger>();
    }

    [Fact]
    public void CreateLogger_ReturnsNullLogger_WhenLogFileIsAlreadyOpened()
    {
        var rootPath = "root";
        var existingLogPath = Path.Combine(
            Directory.GetDirectoryRoot(Directory.GetCurrentDirectory()),
            rootPath,
            "Logs",
            "latest.log");
        var existingLogsContent = "existing logs";

        _diskFileLoggerSettings.Value.Returns(
            new DiskFileLoggerSettings
            {
                Enable = true,
                MaxFilesToKeep = 5
            });
        _hostEnvironment.ContentRootPath.Returns(rootPath);
        _fileSystem.AddFile(existingLogPath, new(existingLogsContent) { AllowedFileShare = FileShare.None });

        var sut = new DiskFileLoggerProvider(_diskFileLoggerSettings, _hostEnvironment, _fileSystem, _timeProvider);
        var logger = sut.CreateLogger("Test");

        logger.Should().BeOfType<NullLogger>();
        _fileSystem.AllFiles.Should().HaveCount(1);
        _fileSystem.AllFiles.ElementAt(0).Should().Be(existingLogPath);
    }

    [Fact]
    public void CreateLogger_ReturnsDiskFileLoggerWithCorrectFilename_WhenDiskFileLoggingIsEnabled()
    {
        var rootPath = "root";
        var expectedPath = Path.Combine(
            Directory.GetDirectoryRoot(Directory.GetCurrentDirectory()),
            rootPath,
            "Logs",
            "latest.log");

        _diskFileLoggerSettings.Value.Returns(
            new DiskFileLoggerSettings
            {
                Enable = true,
                MaxFilesToKeep = 5
            });
        _hostEnvironment.ContentRootPath.Returns(rootPath);

        using var sut = new DiskFileLoggerProvider(
            _diskFileLoggerSettings,
            _hostEnvironment,
            _fileSystem,
            _timeProvider);
        var logger = sut.CreateLogger("Test");

        logger.Should().BeOfType<DiskFileLogger>();
        _fileSystem.AllFiles.Should().ContainSingle(expectedPath);
    }

    [Fact]
    public void CreateLogger_ReturnsDiskFileLoggerAfterOrganisingLogFiles_WhenALogFileExistsAlready()
    {
        var rootPath = "root";
        var latestLogPath = Path.Combine(
            Directory.GetDirectoryRoot(Directory.GetCurrentDirectory()),
            rootPath,
            "Logs",
            "latest.log");
        var existingLogsContent = "existing logs";
        var olderLogFileTimeStamp = new DateTime(2025, 6, 15, 12, 30, 30);
        var olderLogPath = Path.Combine(
            Directory.GetDirectoryRoot(Directory.GetCurrentDirectory()),
            rootPath,
            "Logs",
            $"{olderLogFileTimeStamp:yyyy-MM-dd_HH-mm-ss}.log");
        var currentTime = DateTimeOffset.Now;

        _diskFileLoggerSettings.Value.Returns(
            new DiskFileLoggerSettings
            {
                Enable = true,
                MaxFilesToKeep = 5
            });
        _hostEnvironment.ContentRootPath.Returns(rootPath);
        _fileSystem.AddFile(latestLogPath, new(existingLogsContent));
        _fileSystem.File.SetCreationTime(latestLogPath, olderLogFileTimeStamp);

        _timeProvider.SetLocalTimeZone(TimeZoneInfo.Utc);
        _timeProvider.SetUtcNow(currentTime);

        using var sut = new DiskFileLoggerProvider(
            _diskFileLoggerSettings,
            _hostEnvironment,
            _fileSystem,
            _timeProvider);
        var logger = sut.CreateLogger("Test");

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
        var rootPath = "root";
        var latestLogPath = Path.Combine(
            Directory.GetDirectoryRoot(Directory.GetCurrentDirectory()),
            rootPath,
            "Logs",
            "latest.log");
        var newerLogContent = "newer logs";
        var olderLogContent = "older logs";
        var newerLogTimeStamp = new DateTime(2025, 6, 15, 12, 30, 30);
        var newerLogPath = Path.Combine(
            Directory.GetDirectoryRoot(Directory.GetCurrentDirectory()),
            rootPath,
            "Logs",
            $"{newerLogTimeStamp:yyyy-MM-dd_HH-mm-ss}.log");
        var currentTime = DateTimeOffset.Now;

        _diskFileLoggerSettings.Value.Returns(
            new DiskFileLoggerSettings
            {
                Enable = true,
                MaxFilesToKeep = 2
            });
        _hostEnvironment.ContentRootPath.Returns(rootPath);
        _fileSystem.AddFile(latestLogPath, new(newerLogContent));
        _fileSystem.AddFile(newerLogPath, new(olderLogContent));
        _fileSystem.File.SetCreationTime(latestLogPath, newerLogTimeStamp);
        _fileSystem.File.SetCreationTime(newerLogPath, newerLogTimeStamp.AddDays(-1));

        _timeProvider.SetLocalTimeZone(TimeZoneInfo.Utc);
        _timeProvider.SetUtcNow(currentTime);

        using var sut = new DiskFileLoggerProvider(
            _diskFileLoggerSettings,
            _hostEnvironment,
            _fileSystem,
            _timeProvider);
        var logger = sut.CreateLogger("Test");

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