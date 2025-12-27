using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using System.Reflection;
using VenusRootLoader.BudLoading;
using VenusRootLoader.Public;

namespace VenusRootLoader;

internal sealed class AppDomainEventsHandler
{
    private readonly BudLoaderContext _budLoaderContext;
    private readonly IAssemblyLoader _assemblyLoader;
    private readonly IAppDomainEvents _appDomainEvents;
    private readonly ILogger<AppDomainEventsHandler> _logger;
    private readonly IFileSystem _fileSystem;

    private readonly List<string> _assembliesExtensionPatterns = ["*.dll", "*.exe"];

    public AppDomainEventsHandler(
        BudLoaderContext budLoaderContext,
        IAssemblyLoader assemblyLoader,
        IAppDomainEvents appDomainEvents,
        ILogger<AppDomainEventsHandler> logger,
        IFileSystem fileSystem)
    {
        _budLoaderContext = budLoaderContext;
        _assemblyLoader = assemblyLoader;
        _appDomainEvents = appDomainEvents;
        _logger = logger;
        _fileSystem = fileSystem;
    }

    public void InstallHandlers()
    {
        _appDomainEvents.UnhandledException += OnUnhandledException;
        _appDomainEvents.AssemblyResolve += OnAssemblyResolve;
        _logger.LogDebug("Installed the unhandled exception handler and the assembly resolver");
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
            _budLoaderContext.LoaderPath,
            $"{assemblyName.Name}.dll");
        if (_fileSystem.File.Exists(assemblyFileLoader))
            return _assemblyLoader.LoadFromPath(assemblyFileLoader);

        foreach (string assemblyFile in EnumerateAssembliesFilesRecursivelyFromPath(_budLoaderContext.BudsPath))
        {
            if (_fileSystem.Path.GetFileNameWithoutExtension(assemblyFile) != assemblyName.Name)
                continue;

            _logger.LogDebug("Requested {Name}, loading it from {assemblyFile}", assemblyName.Name, assemblyFile);
            return _assemblyLoader.LoadFromPath(assemblyFile);
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
}