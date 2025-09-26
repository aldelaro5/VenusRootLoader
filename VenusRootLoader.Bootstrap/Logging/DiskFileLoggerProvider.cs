using System.IO.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using VenusRootLoader.Bootstrap.Settings.LogProvider;

namespace VenusRootLoader.Bootstrap.Logging;

public sealed class DiskFileLoggerProvider : ILoggerProvider
{
    private readonly TimeProvider _timeProvider;
    private readonly IFileSystem _fileSystem;
    private readonly StreamWriter? _logWriter;
    private readonly DiskFileLoggerSettings _diskFileLoggerSettings;

    private readonly bool _initialised;
    public DiskFileLoggerProvider(
        IOptions<DiskFileLoggerSettings> loggingSettings,
        IHostEnvironment hostEnvironment,
        IFileSystem fileSystem,
        TimeProvider timeProvider)
    {
        _fileSystem = fileSystem;
        _timeProvider = timeProvider;
        _diskFileLoggerSettings = loggingSettings.Value;
        if (!_diskFileLoggerSettings.Enable!.Value)
            return;

        try
        {
            var logsDirectory = _fileSystem.Path.Combine(hostEnvironment.ContentRootPath, "Logs");
            if (!_fileSystem.Directory.Exists(logsDirectory))
                _fileSystem.Directory.CreateDirectory(logsDirectory);
            PurgeOldLogFiles(logsDirectory, _diskFileLoggerSettings.MaxFilesToKeep!.Value);
            var latestLogFilePath = _fileSystem.Path.Combine(logsDirectory, "latest.log");
            FreeLatestLogFilePath(latestLogFilePath);
            _logWriter = new(_fileSystem.File.Open(latestLogFilePath, FileMode.Create, FileAccess.Write))
            {
                AutoFlush = true
            };
            // For some reason, this isn't done correctly on native Windows so we have to do this to make sure
            _fileSystem.File.SetCreationTime(latestLogFilePath, _timeProvider.GetLocalNow().DateTime);
            _initialised = true;
        }
        catch (IOException e)
        {
            Console.WriteLine($"Disabling the disk file logger due to an IO error, make sure no other instances of the " + $"game are running: {e}");
            _initialised = false;
        }
    }
    public ILogger CreateLogger(string categoryName)
    {
        if (!_diskFileLoggerSettings.Enable!.Value || !_initialised)
            return NullLogger.Instance;
        return new DiskFileLogger(categoryName, _logWriter!,  _timeProvider);
    }

    public void Dispose() => _logWriter?.Dispose();

    private void PurgeOldLogFiles(string logsDirectory, int maxLogFiles)
    {
        var filesTooOld = _fileSystem.Directory
            .GetFiles(logsDirectory, "*.log")
            .OrderByDescending(_fileSystem.File.GetCreationTimeUtc)
            .Skip(maxLogFiles - 1)
            .ToList();
        filesTooOld.ForEach(_fileSystem.File.Delete);
    }

    private void FreeLatestLogFilePath(string latestLogFilePath)
    {
        if (!_fileSystem.File.Exists(latestLogFilePath))
            return;

        var localCreationDateTime = _fileSystem.File.GetCreationTime(latestLogFilePath);
        var logsDirectory = _fileSystem.Path.GetDirectoryName(latestLogFilePath)!;
        _fileSystem.File.Move(latestLogFilePath, _fileSystem.Path.Combine(logsDirectory, $"{localCreationDateTime:yyyy-MM-dd_HH-mm-ss}.log"));
    }
}