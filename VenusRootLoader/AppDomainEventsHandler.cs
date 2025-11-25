using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using System.Reflection;

namespace VenusRootLoader;

internal sealed class AppDomainEventsHandler : IHostedService
{
    private readonly ModLoaderContext _modLoaderContext;
    private readonly ILogger<AppDomainEventsHandler> _logger;
    private readonly IFileSystem _fileSystem;

    private readonly List<string> _assembliesExtensionPatterns = ["*.dll", "*.exe"];

    public AppDomainEventsHandler(
        ModLoaderContext modLoaderContext,
        ILogger<AppDomainEventsHandler> logger,
        IFileSystem fileSystem)
    {
        _modLoaderContext = modLoaderContext;
        _logger = logger;
        _fileSystem = fileSystem;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
        return Task.CompletedTask;
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
            _logger.LogCritical(ex, "Unhandled exception");
        else
            _logger.LogCritical("Unhandled exception: {exception}", e.ExceptionObject);
    }

    private Assembly? OnAssemblyResolve(object sender, ResolveEventArgs args)
    {
        AssemblyName assemblyName = new(args.Name);

        string assemblyFileLoader = _fileSystem.Path.Combine(
            _modLoaderContext.LoaderPath,
            $"{assemblyName.Name}.dll");
        if (_fileSystem.File.Exists(assemblyFileLoader))
            return Assembly.LoadFrom(assemblyFileLoader);

        foreach (string assemblyFile in EnumerateAssembliesFilesRecursivelyFromPath(_modLoaderContext.ModsPath))
        {
            if (_fileSystem.Path.GetFileNameWithoutExtension(assemblyFile) != assemblyName.Name)
                continue;

            _logger.LogDebug("Requested {Name}, loading it at {assemblyFile}", assemblyName.Name, assemblyFile);
            return Assembly.LoadFrom(assemblyFile);
        }

        _logger.LogWarning(
            "Unable to resolve the assembly: {Name}. If this is an optional dependency, this warning can be ignored",
            args.Name);
        return null;
    }

    private IEnumerable<string> EnumerateAssembliesFilesRecursivelyFromPath(string path)
    {
        foreach (string extensionPattern in _assembliesExtensionPatterns)
        {
            foreach (string assemblyFile in _fileSystem.Directory.EnumerateFiles(
                         path,
                         extensionPattern,
                         SearchOption.AllDirectories))
            {
                yield return assemblyFile;
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}