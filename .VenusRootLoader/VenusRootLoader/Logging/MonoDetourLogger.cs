using Microsoft.Extensions.Logging;
using static MonoDetour.Logging.MonoDetourLogger;

namespace VenusRootLoader.Logging;

internal sealed class MonoDetourLogger
{
    private static readonly Dictionary<LogChannel, LogLevel> LogLevelMappings = new()
    {
        [LogChannel.Info] = LogLevel.Information,
        [LogChannel.Warning] = LogLevel.Warning,
        [LogChannel.Error] = LogLevel.Error,
        [LogChannel.Debug] = LogLevel.Debug,
        [LogChannel.IL] = LogLevel.Trace
    };

    private readonly ILogger<MonoDetourLogger> _monoDetourLogger;
    private readonly ILogger _monoDetourManagerLogger;

    public MonoDetourLogger(ILogger<MonoDetourLogger> monoDetourLogger, ILoggerFactory factory)
    {
        _monoDetourLogger = monoDetourLogger;
        _monoDetourManagerLogger = factory.CreateLogger($"{nameof(VenusRootLoader)}-{nameof(MonoDetour)}");
    }

    internal void InstallMonoDetourLogger()
    {
        LogChannel managerFilteredChannels = GetManagerLogChannels();
        MonoDetour.HookGen.DefaultMonoDetourManager.Instance.LogFilter = managerFilteredChannels;
        LogToConsole = false;
        OnLog += LoggerOnMessageReceived;
    }

    private void LoggerOnMessageReceived(LogChannel channel, string message) =>
        _monoDetourLogger.Log(LogLevelMappings[channel], message);

    private LogChannel GetManagerLogChannels()
    {
        LogChannel managerFilteredChannels = LogChannel.None;
        if (_monoDetourManagerLogger.IsEnabled(LogLevel.Error))
            managerFilteredChannels |= LogChannel.Error;
        if (_monoDetourManagerLogger.IsEnabled(LogLevel.Warning))
            managerFilteredChannels |= LogChannel.Warning;
        if (_monoDetourManagerLogger.IsEnabled(LogLevel.Information))
            managerFilteredChannels |= LogChannel.Info;
        if (_monoDetourManagerLogger.IsEnabled(LogLevel.Debug))
            managerFilteredChannels |= LogChannel.Debug;
        if (_monoDetourManagerLogger.IsEnabled(LogLevel.Trace))
            managerFilteredChannels |= LogChannel.IL;

        return managerFilteredChannels;
    }
}