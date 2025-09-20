using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using VenusRootLoader.Bootstrap.Settings.LogProvider;

namespace VenusRootLoader.Bootstrap.Logging;

public sealed class DiskFileLoggerProvider :  ILoggerProvider
{
    private readonly StreamWriter? _logWriter;
    private readonly DiskFileLoggerSettings _diskFileLoggerSettings;

    private readonly bool _initialised;

    public DiskFileLoggerProvider(
        IOptions<DiskFileLoggerSettings> loggingSettings,
        IHostEnvironment hostEnvironment)
    {
        _diskFileLoggerSettings = loggingSettings.Value;
        if (!_diskFileLoggerSettings.Enable!.Value)
            return;

        try
        {
            var logsDirectory = Path.Combine(hostEnvironment.ContentRootPath, "Logs");
            if (!Directory.Exists(logsDirectory))
                Directory.CreateDirectory(logsDirectory);

            PurgeOldLogFiles(logsDirectory, _diskFileLoggerSettings.MaxFilesToKeep!.Value);
            var latestLogFilePath = Path.Combine(logsDirectory, "latest.log");
            FreeLatestLogFilePath(latestLogFilePath);
            _logWriter = new(File.Open(latestLogFilePath, FileMode.Create, FileAccess.Write))
            {
                AutoFlush = true
            };
            // For some reason, this isn't done correctly on native Windows so we have to do this to make sure
            File.SetCreationTime(latestLogFilePath, DateTime.Now);
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
        if (!_diskFileLoggerSettings.Enable!.Value || !_initialised)
            return NullLogger.Instance;
        return new DiskFileLogger(categoryName, _logWriter!);
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