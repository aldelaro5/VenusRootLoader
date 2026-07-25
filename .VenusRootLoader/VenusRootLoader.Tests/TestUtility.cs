using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace VenusRootLoader.Tests;

public static class TestUtility
{
    public static void AssertErrorLogs(
        FakeLogger logger,
        int expectedErrorLogsAmount,
        string expectedExceptionMessageTemplate = "*")
    {
        using AssertionScope scope = new();
        List<FakeLogRecord> errorLogs = logger.Collector.GetSnapshot().Where(l => l.Level == LogLevel.Error).ToList();
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
}