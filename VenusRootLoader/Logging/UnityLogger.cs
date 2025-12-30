using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Text;
using UnityEngine;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace VenusRootLoader.Logging;

internal sealed class UnityLogger
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger _logger;

    public UnityLogger(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
        _logger = _loggerFactory.CreateLogger("UNITY");
    }

    internal void InstallManagedUnityLogger()
    {
        // This means the bootstrap isn't already capturing every log including native ones.
        // If it is, we don't want to duplicate every log with this logger.
        if (!_logger.IsEnabled(LogLevel.Trace))
            Application.logMessageReceivedThreaded += ApplicationOnLogMessageReceived;
    }

    private void ApplicationOnLogMessageReceived(string condition, string stackTrace, LogType type)
    {
        LogLevel level = type switch
        {
            LogType.Error => LogLevel.Error,
            LogType.Assert => LogLevel.Debug,
            LogType.Warning => LogLevel.Warning,
            LogType.Log => LogLevel.Information,
            LogType.Exception => LogLevel.Critical,
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<LogLevel>(nameof(type))
        };

        StringBuilder sb = new();
        sb.Append(condition);
        if (!string.IsNullOrWhiteSpace(stackTrace))
        {
            sb.Append('\n');
            sb.Append(stackTrace);
        }

        _logger.Log(level, sb.ToString());
    }
}