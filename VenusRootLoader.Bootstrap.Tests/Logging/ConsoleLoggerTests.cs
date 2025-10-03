using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using Pastel;
using System.Drawing;
using VenusRootLoader.Bootstrap.Logging;
using VenusRootLoader.Bootstrap.Tests.TestHelpers;

namespace VenusRootLoader.Bootstrap.Tests.Logging;

public class ConsoleLoggerTests
{
    private readonly FakeTimeProvider _timeProvider = new();

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
        var writer = new StringWriter();
        Console.SetOut(writer);

        ConsoleLogger sut = new(category, ConsoleLogProvider.RenderingMode.NoColors, _timeProvider);
        sut.Log(logLevel, message);
        var result = writer.ToString();

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
        var writer = new StringWriter();
        Console.SetOut(writer);

        ConsoleLogger sut = new(category, ConsoleLogProvider.RenderingMode.NoColors, _timeProvider);
        sut.LogInformation("Some logging message");
        var result = writer.ToString();

        result.Should().Contain(expected);
        result.Should().NotContain(category);
    }

    [Fact]
    public void Log_LogsWithMessageAndException_WhenLoggingException()
    {
        var message = "Some logging message";
        var exception = new Exception("Some exception message");
        var writer = new StringWriter();
        Console.SetOut(writer);

        ConsoleLogger sut = new("Some category", ConsoleLogProvider.RenderingMode.NoColors, _timeProvider);
        sut.LogInformation(exception, message);
        var result = writer.ToString();

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
        var writer = new StringWriter();
        Console.SetOut(writer);

        ConsoleLogger sut = new(category, ConsoleLogProvider.RenderingMode.AnsiColors, _timeProvider);
        sut.Log(logLevel, message);
        var result = writer.ToString();

        result.Should().Contain($"[{timeStampString.Pastel(Color.LimeGreen)}]");
        result.Should().Contain($"[{levelMoniker.Pastel(levelColor)}]");
        result.Should().Contain($"[{category.Pastel(Color.White)}]");
        result.Should().Contain($"{message.Pastel(levelColor)}");
    }

    public static List<object[]> LogCategoriesTestDataAnsi =>
    [
        ["Some category", Color.White],
        ["UNITY", Color.Cyan],
        [typeof(ConsoleLogger).Assembly.GetName().Name!, Color.Magenta]
    ];

    [Theory]
    [MemberData(nameof(LogCategoriesTestDataAnsi))]
    public void Log_LogsCategoryWithTheCorrectColors_WhenCalledInAnsiMode(string categoryName, Color levelColor)
    {
        var writer = new StringWriter();
        Console.SetOut(writer);
        var simplifiedCategoryName = categoryName;
        var lastDotIndex = categoryName.LastIndexOf('.');
        if (lastDotIndex > -1)
            simplifiedCategoryName = categoryName[(lastDotIndex + 1)..];

        ConsoleLogger sut = new(categoryName, ConsoleLogProvider.RenderingMode.AnsiColors, _timeProvider);
        sut.LogInformation("Some logging message");
        var result = writer.ToString();

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
        var timeStamp = DateTimeOffset.Now;
        var timeStampString = timeStamp.ToString("HH:mm:ss.fff");
        _timeProvider.AdjustTime(timeStamp);
        var category = "Some category";
        var message = "Some logging message";
        var writer = new LegacyConsoleColorsCaptureTextWriter();
        Console.SetOut(writer);

        ConsoleLogger sut = new(category, ConsoleLogProvider.RenderingMode.LegacyColors, _timeProvider);
        sut.Log(logLevel, message);
        var result = writer.ToString();

        result.Should().Contain($"[~{nameof(ConsoleColor.Green)}~{timeStampString}~~]");
        result.Should().Contain($"[~{levelColor.ToString()}~{levelMoniker}~~]");
        result.Should().Contain($"[~{nameof(ConsoleColor.White)}~{category}~~]");
        result.Should().Contain($"~{levelColor.ToString()}~{message}");

        Console.ForegroundColor.Should().Be((ConsoleColor)(-1));
    }

    public static List<object[]> LogCategoriesTestDataLegacy =>
    [
        ["Some category", ConsoleColor.White],
        ["UNITY", ConsoleColor.Cyan],
        [typeof(ConsoleLogger).Assembly.GetName().Name!, ConsoleColor.Magenta]
    ];

    [Theory]
    [MemberData(nameof(LogCategoriesTestDataLegacy))]
    public void Log_LogsCategoryWithTheCorrectColors_WhenCalledInLegacyMode(
        string categoryName,
        ConsoleColor levelColor)
    {
        var writer = new LegacyConsoleColorsCaptureTextWriter();
        Console.SetOut(writer);
        var simplifiedCategoryName = categoryName;
        var lastDotIndex = categoryName.LastIndexOf('.');
        if (lastDotIndex > -1)
            simplifiedCategoryName = categoryName[(lastDotIndex + 1)..];

        ConsoleLogger sut = new(categoryName, ConsoleLogProvider.RenderingMode.LegacyColors, _timeProvider);
        sut.LogInformation("Some logging message");
        var result = writer.ToString();

        result.Should().Contain($"[~{levelColor.ToString()}~{simplifiedCategoryName}~~]");
    }
}