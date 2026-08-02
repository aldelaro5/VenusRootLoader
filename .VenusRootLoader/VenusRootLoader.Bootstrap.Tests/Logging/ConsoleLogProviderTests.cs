using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using System.Runtime.CompilerServices;
using VenusRootLoader.Bootstrap.Logging;
using VenusRootLoader.Bootstrap.Settings.LogProvider;
using VenusRootLoader.Bootstrap.Shared;
using Windows.Win32.Foundation;
using Windows.Win32.System.Console;

namespace VenusRootLoader.Bootstrap.Tests.Logging;

public sealed class ConsoleLogProviderTests
{
    private readonly IOptions<ConsoleLoggerSettings> _consoleLoggerOptions =
        Substitute.For<IOptions<ConsoleLoggerSettings>>();

    private readonly TimeProvider _timeProvider = Substitute.For<TimeProvider>();
    private readonly IWin32 _win32 = Substitute.For<IWin32>();

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_renderingMode")]
    private static extern ref ConsoleLogProvider.RenderingMode ConsoleLogProviderRenderingMode(ConsoleLogger provider);

    [Fact]
    public void CreateLogger_ReturnsNullLogger_WhenConsoleLoggerIsDisabled()
    {
        _consoleLoggerOptions.Value.Returns(
            new ConsoleLoggerSettings
            {
                Enable = false,
                LogWithColors = true
            });
        GameExecutionContext gameExecutionContext = new GameExecutionContext
        {
            GameDir = "",
            DataDir = "",
            UnityPlayerDllFileName = "UnityPlayer.dll",
            IsWine = false
        };

        using ConsoleLogProvider sut = new ConsoleLogProvider(
            gameExecutionContext,
            _consoleLoggerOptions,
            _win32,
            _timeProvider);
        ILogger logger = sut.CreateLogger("category");

        logger.Should().BeOfType<NullLogger>();
    }

    [Fact]
    public void CreateLogger_ReturnsConsoleLoggerWithNoColors_WhenColorsAreDisabled()
    {
        _consoleLoggerOptions.Value.Returns(
            new ConsoleLoggerSettings
            {
                Enable = true,
                LogWithColors = false
            });
        GameExecutionContext gameExecutionContext = new GameExecutionContext
        {
            GameDir = "",
            DataDir = "",
            UnityPlayerDllFileName = "UnityPlayer.dll",
            IsWine = false
        };

        using ConsoleLogProvider sut = new ConsoleLogProvider(
            gameExecutionContext,
            _consoleLoggerOptions,
            _win32,
            _timeProvider);
        ILogger logger = sut.CreateLogger("category");

        logger.Should().BeOfType<ConsoleLogger>();
        ConsoleLogProviderRenderingMode((ConsoleLogger)logger)
            .Should().Be(ConsoleLogProvider.RenderingMode.NoColors);
    }

    [Theory]
    [InlineData(STD_HANDLE.STD_OUTPUT_HANDLE)]
    [InlineData(STD_HANDLE.STD_ERROR_HANDLE)]
    public void CreateLogger_ReturnsConsoleLoggerWithLegacyColors_WhenColorsAreEnabledAndAnsiIsNotSupported(
        STD_HANDLE stdHandleWithoutAnsi)
    {
        HANDLE handleWithoutAnsi = (HANDLE)Random.Shared.Next();
        _consoleLoggerOptions.Value.Returns(
            new ConsoleLoggerSettings
            {
                Enable = true,
                LogWithColors = true
            });
        GameExecutionContext gameExecutionContext = new GameExecutionContext
        {
            GameDir = "",
            DataDir = "",
            UnityPlayerDllFileName = "UnityPlayer.dll",
            IsWine = false
        };

        _win32.GetStdHandle(stdHandleWithoutAnsi).Returns(handleWithoutAnsi);
        _win32.SetConsoleMode(handleWithoutAnsi, Arg.Any<CONSOLE_MODE>()).Returns((BOOL)false);

        using ConsoleLogProvider sut = new ConsoleLogProvider(
            gameExecutionContext,
            _consoleLoggerOptions,
            _win32,
            _timeProvider);
        ILogger logger = sut.CreateLogger("category");

        logger.Should().BeOfType<ConsoleLogger>();
        ConsoleLogProviderRenderingMode((ConsoleLogger)logger)
            .Should().Be(ConsoleLogProvider.RenderingMode.LegacyColors);
    }

    [Fact]
    public void CreateLogger_ReturnsConsoleLoggerWithLegacyColors_WhenUsingWine()
    {
        HANDLE stdOutHandle = (HANDLE)Random.Shared.Next();
        HANDLE stdErrHandle = (HANDLE)Random.Shared.Next();
        _consoleLoggerOptions.Value.Returns(
            new ConsoleLoggerSettings
            {
                Enable = true,
                LogWithColors = true
            });
        GameExecutionContext gameExecutionContext = new GameExecutionContext
        {
            GameDir = "",
            DataDir = "",
            UnityPlayerDllFileName = "UnityPlayer.dll",
            IsWine = true
        };
        _win32.GetStdHandle(STD_HANDLE.STD_OUTPUT_HANDLE).Returns(stdOutHandle);
        _win32.GetStdHandle(STD_HANDLE.STD_ERROR_HANDLE).Returns(stdErrHandle);
        _win32.SetConsoleMode(stdOutHandle, Arg.Any<CONSOLE_MODE>()).Returns((BOOL)true);
        _win32.SetConsoleMode(stdErrHandle, Arg.Any<CONSOLE_MODE>()).Returns((BOOL)true);

        using ConsoleLogProvider sut = new ConsoleLogProvider(
            gameExecutionContext,
            _consoleLoggerOptions,
            _win32,
            _timeProvider);
        ILogger logger = sut.CreateLogger("category");

        logger.Should().BeOfType<ConsoleLogger>();
        ConsoleLogProviderRenderingMode((ConsoleLogger)logger)
            .Should().Be(ConsoleLogProvider.RenderingMode.LegacyColors);
    }

    [Fact]
    public void CreateLogger_ReturnsConsoleLoggerWithAnsiColors_WhenSupportedAndNotUsingWine()
    {
        HANDLE stdOutHandle = (HANDLE)Random.Shared.Next();
        HANDLE stdErrHandle = (HANDLE)Random.Shared.Next();
        _consoleLoggerOptions.Value.Returns(
            new ConsoleLoggerSettings
            {
                Enable = true,
                LogWithColors = true
            });
        GameExecutionContext gameExecutionContext = new GameExecutionContext
        {
            GameDir = "",
            DataDir = "",
            UnityPlayerDllFileName = "UnityPlayer.dll",
            IsWine = false
        };
        _win32.GetStdHandle(STD_HANDLE.STD_OUTPUT_HANDLE).Returns(stdOutHandle);
        _win32.GetStdHandle(STD_HANDLE.STD_ERROR_HANDLE).Returns(stdErrHandle);
        _win32.SetConsoleMode(stdOutHandle, Arg.Any<CONSOLE_MODE>()).Returns((BOOL)true);
        _win32.SetConsoleMode(stdErrHandle, Arg.Any<CONSOLE_MODE>()).Returns((BOOL)true);

        using ConsoleLogProvider sut = new ConsoleLogProvider(
            gameExecutionContext,
            _consoleLoggerOptions,
            _win32,
            _timeProvider);
        ILogger logger = sut.CreateLogger("category");

        logger.Should().BeOfType<ConsoleLogger>();
        ConsoleLogProviderRenderingMode((ConsoleLogger)logger)
            .Should().Be(ConsoleLogProvider.RenderingMode.AnsiColors);
    }
}