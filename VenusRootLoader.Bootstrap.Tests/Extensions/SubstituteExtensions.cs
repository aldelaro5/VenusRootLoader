using Microsoft.Extensions.Logging;
using NSubstitute;

namespace VenusRootLoader.Bootstrap.Tests.Extensions;

public static class SubstituteExtensions
{
    public static void ReceivedLog(this ILogger logger, int amount, LogLevel logLevel)
    {
        logger.Received(amount).Log(
            logLevel,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    public static void ReceivedLog(this ILogger logger, int amount, LogLevel logLevel, Func<object, bool> logPredicate)
    {
        logger.Received(amount).Log(
            logLevel,
            Arg.Any<EventId>(),
            Arg.Is<object>(log => logPredicate(log)),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
}