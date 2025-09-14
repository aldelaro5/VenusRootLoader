using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using VenusRootLoader.Bootstrap.Settings;
using VenusRootLoader.Bootstrap.Shared;

namespace VenusRootLoader.Bootstrap.Logging;

public class FileLoggerProvider :  ILoggerProvider
{
    private readonly GameExecutionContext _gameExecutionContext;
    private readonly StreamWriter? _logWriter;
    private readonly LoggingSettings _loggingSettings;
    
    private readonly bool _initialised;

    public FileLoggerProvider(GameExecutionContext gameExecutionContext, IOptions<LoggingSettings> loggingSettings)
    {
        _gameExecutionContext = gameExecutionContext;
        _loggingSettings = loggingSettings.Value;
        if (!_loggingSettings.EnableDiskLogging!.Value)
            return;

        try
        {
            var logsDirectory = Path.Combine(_gameExecutionContext.GameDir, "Logs");
            if (!Directory.Exists(logsDirectory))
                Directory.CreateDirectory(logsDirectory);

            PurgeOldLogFiles(logsDirectory, _loggingSettings.DiskLoggingMaxFiles!.Value);
            var latestLogFilePath = Path.Combine(logsDirectory, "latest.log");
            FreeLatestLogFilePath(latestLogFilePath);
            _logWriter = new(File.Open(latestLogFilePath, FileMode.Create, FileAccess.Write))
            {
                AutoFlush = true
            };
            _initialised = true;
        }
        catch (IOException e)
        {
            Console.WriteLine($"Disabling the disk file logger due to an IO error, make sure no other instances of the " +
                              $"game are running: {e}");
            _initialised = false;
        }
    }

    public ILogger CreateLogger(string categoryName)
    {
        if (!_loggingSettings.EnableDiskLogging!.Value || !_initialised)
            return NullLogger.Instance;
        return new FileLogger(categoryName, _logWriter!);
    }

    public void Dispose() => _logWriter?.Dispose();

    private static void PurgeOldLogFiles(string logsDirectory, int maxLogFiles)
    {
        var filesTooOld = Directory.GetFiles(logsDirectory, "*.log")
            .OrderByDescending(File.GetCreationTimeUtc)
            .Skip(maxLogFiles - 1)
            .ToList();
        filesTooOld.ForEach(File.Delete);
    }

    private static void FreeLatestLogFilePath(string latestLogFilePath)
    {
        if (!File.Exists(latestLogFilePath))
            return;

        var localCreationDateTime = File.GetCreationTime(latestLogFilePath);
        var logsDirectory = Path.GetDirectoryName(latestLogFilePath)!;
        File.Move(latestLogFilePath, Path.Combine(logsDirectory, $"{localCreationDateTime:yyyy-MM-dd_HH-mm-ss-FFF}.log"));
    }
}