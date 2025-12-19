using HarmonyLib.Tools;
using Microsoft.Extensions.Logging;

namespace VenusRootLoader.Logging;

internal sealed class HarmonyLogger
{
    private static readonly Dictionary<Logger.LogChannel, LogLevel> LogLevelMappings = new()
    {
        [Logger.LogChannel.IL] = LogLevel.Trace,
        [Logger.LogChannel.Debug] = LogLevel.Debug,
        [Logger.LogChannel.Info] = LogLevel.Information,
        [Logger.LogChannel.Warn] = LogLevel.Warning,
        [Logger.LogChannel.Error] = LogLevel.Error,
        [Logger.LogChannel.None] = LogLevel.None
    };

    private readonly ILogger<HarmonyLogger> _harmonyLogger;

    public HarmonyLogger(ILogger<HarmonyLogger> logger)
    {
        _harmonyLogger = logger;
    }

    internal void InstallHarmonyLogging()
    {
        Logger.ChannelFilter = Logger.LogChannel.All;
        Logger.MessageReceived += LoggerOnMessageReceived;
    }

    private void LoggerOnMessageReceived(object sender, Logger.LogEventArgs e) =>
        _harmonyLogger.Log(LogLevelMappings[e.LogChannel], e.Message);
}