using HarmonyLib.Tools;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace VenusRootLoader.Preloader;

internal class HarmonyLogger : IHostedService
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

    public HarmonyLogger(ILogger<HarmonyLogger> bootstrapFunctions)
    {
        _harmonyLogger = bootstrapFunctions;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Logger.ChannelFilter = Logger.LogChannel.All;
        Logger.MessageReceived += LoggerOnMessageReceived;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private void LoggerOnMessageReceived(object sender, Logger.LogEventArgs e) =>
        _harmonyLogger.Log(LogLevelMappings[e.LogChannel], e.Message);
}