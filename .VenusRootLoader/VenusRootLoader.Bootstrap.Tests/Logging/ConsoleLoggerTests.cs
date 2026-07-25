using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using Pastel;
using System.Drawing;
using VenusRootLoader.Bootstrap.Logging;
using VenusRootLoader.Bootstrap.Tests.TestHelpers;

namespace VenusRootLoader.Bootstrap.Tests.Logging;

public sealed class ConsoleLoggerTests
{
    private readonly FakeTimeProvider _timeProvider = new();
    private readonly StringWriter _writer = new();
    private readonly TestConsole _console;

    public ConsoleLoggerTests() => _console = new(_writer);

    [Theory]
    [InlineData(LogLevel.Trace, "T")]
    [InlineData(LogLevel.Debug, "D")]
    [InlineData(LogLevel.Information, "I")]
    [InlineData(LogLevel.Warning, "W")]
    [InlineData(LogLevel.Error, "E")]
    [InlineData(LogLevel.Critical, "!")]
    public void Log_LogsWithTheCorrectInformation_WhenCalled(LogLevel logLevel, string levelMoniker)
    {
        var timeStamp = DateTimeOffset.Now;
        _timeProvider.AdjustTime(timeStamp);
        var category = "Some category";
        var message = "Some logging message";

        ConsoleLogger sut = new(category, ConsoleLogProvider.RenderingMode.NoColors, _timeProvider, _console);
        sut.Log(logLevel, message);
        var result = _writer.ToString();

        result.Should().Contain(timeStamp.ToString("HH:mm:ss.fff"));
        result.Should().Contain($"[{levelMoniker}]");
        result.Should().Contain(category);
        result.Should().Contain(message);
    }

    [Theory]
    [InlineData("first.second", "second")]
    [InlineData("first.second.third", "third")]
    public void Log_LogsWithSimplifiedCategory_WhenItContainsDots(string category, string expected)
    {
        ConsoleLogger sut = new(category, ConsoleLogProvider.RenderingMode.NoColors, _timeProvider, _console);
        sut.LogInformation("Some logging message");
        var result = _writer.ToString();

        result.Should().Contain(expected);
        result.Should().NotContain(category);
    }

    [Fact]
    public void Log_LogsWithMessageAndException_WhenLoggingException()
    {
        var message = "Some logging message";
        var exception = new Exception("Some exception message");

        ConsoleLogger sut = new("Some category", ConsoleLogProvider.RenderingMode.NoColors, _timeProvider, _console);
        sut.LogInformation(exception, message);
        var result = _writer.ToString();

        result.Should().Contain(message);
        result.Should().Contain(exception.ToString());
    }

    public static List<object[]> LogLevelsTestDataAnsi =>
    [
        [LogLevel.Trace, "T", Color.DimGray],
        [LogLevel.Debug, "D", Color.Gray],
        [LogLevel.Information, "I", Color.White],
        [LogLevel.Warning, "W", Color.Yellow],
        [LogLevel.Error, "E", Color.Red],
        [LogLevel.Critical, "!", Color.Red]
    ];

    [Theory]
    [MemberData(nameof(LogLevelsTestDataAnsi))]
    public void Log_LogsWithTheCorrectColors_WhenCalledInAnsiMode(
        LogLevel logLevel,
        string levelMoniker,
        Color levelColor)
    {
        var timeStamp = DateTimeOffset.Now;
        var timeStampString = timeStamp.ToString("HH:mm:ss.fff");
        _timeProvider.AdjustTime(timeStamp);
        var category = "Some category";
        var message = "Some logging message";

        ConsoleLogger sut = new(category, ConsoleLogProvider.RenderingMode.AnsiColors, _timeProvider, _console);
        sut.Log(logLevel, message);
        var result = _writer.ToString();

        result.Should().Contain($"[{timeStampString}]");
        result.Should().Contain($"[{levelMoniker.Pastel(levelColor)}]");
        result.Should().Contain($"[{category.Pastel(Color.Cyan)}]");
        result.Should().Contain($"{message.Pastel(levelColor)}");
    }

    public static List<object[]> LogCategoriesTestDataAnsi =>
    [
        ["Some category", Color.Cyan],
        ["UNITY", Color.LimeGreen],
        ["VenusRootLoader.something", Color.CornflowerBlue],
        [typeof(ConsoleLogger).Assembly.GetName().Name!, Color.Magenta]
    ];

    [Theory]
    [MemberData(nameof(LogCategoriesTestDataAnsi))]
    public void Log_LogsCategoryWithTheCorrectColors_WhenCalledInAnsiMode(string categoryName, Color levelColor)
    {
        var simplifiedCategoryName = categoryName;
        var lastDotIndex = categoryName.LastIndexOf('.');
        if (lastDotIndex > -1)
            simplifiedCategoryName = categoryName[(lastDotIndex + 1)..];

        ConsoleLogger sut = new(categoryName, ConsoleLogProvider.RenderingMode.AnsiColors, _timeProvider, _console);
        sut.LogInformation("Some logging message");
        var result = _writer.ToString();

        result.Should().Contain($"[{simplifiedCategoryName.Pastel(levelColor)}]");
    }

    public static List<object[]> LogLevelsTestDataLegacy =>
    [
        [LogLevel.Trace, "T", ConsoleColor.DarkGray],
        [LogLevel.Debug, "D", ConsoleColor.Gray],
        [LogLevel.Information, "I", ConsoleColor.White],
        [LogLevel.Warning, "W", ConsoleColor.Yellow],
        [LogLevel.Error, "E", ConsoleColor.Red],
        [LogLevel.Critical, "!", ConsoleColor.Red]
    ];

    [Theory]
    [MemberData(nameof(LogLevelsTestDataLegacy))]
    public void Log_LogsWithTheCorrectColors_WhenCalledInLegacyMode(
        LogLevel logLevel,
        string levelMoniker,
        ConsoleColor levelColor)
    {
        _console.WriteLegacyColorMarkers = true;
        var timeStamp = DateTimeOffset.Now;
        var timeStampString = timeStamp.ToString("HH:mm:ss.fff");
        _timeProvider.AdjustTime(timeStamp);
        var category = "Some category";
        var message = "Some logging message";

        ConsoleLogger sut = new(category, ConsoleLogProvider.RenderingMode.LegacyColors, _timeProvider, _console);
        sut.Log(logLevel, message);
        var result = _writer.ToString();

        result.Should().Contain($"~~[{timeStampString}]");
        result.Should().Contain($"[~{levelColor.ToString()}~{levelMoniker}~~]");
        result.Should().Contain($"[~{nameof(ConsoleColor.Cyan)}~{category}~~]");
        result.Should().Contain($"~{levelColor.ToString()}~{message}");

        _console.ForegroundColor.Should().Be((ConsoleColor)(-1));
    }

    public static List<object[]> LogCategoriesTestDataLegacy =>
    [
        ["Some category", ConsoleColor.Cyan],
        ["UNITY", ConsoleColor.Green],
        ["VenusRootLoader.something", ConsoleColor.Blue],
        [typeof(ConsoleLogger).Assembly.GetName().Name!, ConsoleColor.Magenta]
    ];

    [Theory]
    [MemberData(nameof(LogCategoriesTestDataLegacy))]
    public void Log_LogsCategoryWithTheCorrectColors_WhenCalledInLegacyMode(
        string categoryName,
        ConsoleColor levelColor)
    {
        _console.WriteLegacyColorMarkers = true;
        var simplifiedCategoryName = categoryName;
        var lastDotIndex = categoryName.LastIndexOf('.');
        if (lastDotIndex > -1)
            simplifiedCategoryName = categoryName[(lastDotIndex + 1)..];

        ConsoleLogger sut = new(categoryName, ConsoleLogProvider.RenderingMode.LegacyColors, _timeProvider, _console);
        sut.LogInformation("Some logging message");
        var result = _writer.ToString();

        result.Should().Contain($"[~{levelColor.ToString()}~{simplifiedCategoryName}~~]");
    }
}