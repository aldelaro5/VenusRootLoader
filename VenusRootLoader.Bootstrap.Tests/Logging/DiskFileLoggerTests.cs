using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using System.Text;
using VenusRootLoader.Bootstrap.Logging;

namespace VenusRootLoader.Bootstrap.Tests.Logging;

public class DiskFileLoggerTests
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
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream)
        {
            AutoFlush = true
        };

        DiskFileLogger sut = new(category, writer, _timeProvider);
        sut.Log(logLevel, message);
        var result = Encoding.UTF8.GetString(stream.ToArray());

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
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream)
        {
            AutoFlush = true
        };

        DiskFileLogger sut = new(category, writer, _timeProvider);
        sut.LogInformation("Some logging message");
        var result = Encoding.UTF8.GetString(stream.ToArray());

        result.Should().Contain(expected);
        result.Should().NotContain(category);
    }

    [Fact]
    public void Log_LogsWithMessageAndException_WhenLoggingException()
    {
        var message = "Some logging message";
        var exception = new Exception("Some exception message");
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream)
        {
            AutoFlush = true
        };

        DiskFileLogger sut = new("Some category", writer, _timeProvider);
        sut.LogInformation(exception, message);
        var result = Encoding.UTF8.GetString(stream.ToArray());

        result.Should().Contain(message);
        result.Should().Contain(exception.ToString());
    }
}