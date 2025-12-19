using AsmResolver.DotNet;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Text.Json;
using VenusRootLoader.JsonConverters;
using VenusRootLoader.Models;

namespace VenusRootLoader.BudLoading;

internal interface IBudsDiscoverer
{
    IList<BudInfo> DiscoverAllBudsFromDisk();
}

internal sealed class BudsDiscoverer : IBudsDiscoverer
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<BudsDiscoverer> _logger;
    private readonly BudLoaderContext _budLoaderContext;

    public BudsDiscoverer(
        IFileSystem fileSystem,
        ILogger<BudsDiscoverer> logger,
        BudLoaderContext budLoaderContext)
    {
        _fileSystem = fileSystem;
        _logger = logger;
        _budLoaderContext = budLoaderContext;
    }

    public IList<BudInfo> DiscoverAllBudsFromDisk()
    {
        List<BudInfo> result = [];

        foreach (string budDirectory in _fileSystem.Directory.EnumerateDirectories(_budLoaderContext.BudsPath))
        {
            string manifestPath = _fileSystem.Path.Combine(budDirectory, "manifest.json");
            if (!_fileSystem.File.Exists(manifestPath))
                continue;

            try
            {
                string manifestContent = _fileSystem.File.ReadAllText(manifestPath);
                BudManifest? budManifest = JsonSerializer.Deserialize<BudManifest>(
                    manifestContent,
                    new JsonSerializerOptions
                    {
                        Converters =
                        {
                            NuGetVersionJsonConverter.Instance,
                            NuGetVersionRangeJsonConverter.Instance
                        }
                    });
                if (budManifest is null)
                    throw new JsonException("The bud manifest deserialized to null");

                EnsureBudManifestIsValid(budManifest);

                string budAssemblyPath = _fileSystem.Path.Combine(budDirectory, budManifest.AssemblyName);
                if (!_fileSystem.File.Exists(budAssemblyPath))
                    throw new FileNotFoundException("The bud assembly file does not exist", budAssemblyPath);

                if (TryValidateBudAssembly(budAssemblyPath, out TypeDefinition? budType))
                {
                    BudInfo newBud = new()
                    {
                        BudManifest = budManifest,
                        BudAssemblyPath = budAssemblyPath,
                        BudType = budType
                    };
                    result.Add(newBud);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(
                    e,
                    "An exception occurred while reading the bud manifest located at {manifestPath}",
                    manifestPath);
            }
        }

        if (result.Count == 0)
        {
            _logger.LogDebug("Discovered no buds");
        }
        else
        {
            _logger.LogDebug(
                "Discovered {budCount} buds:\n\n{buds}",
                result.Count,
                string.Join(", ", result.Select(m => m.BudManifest.BudId)));
        }
        return result;
    }

    private static void EnsureBudManifestIsValid(BudManifest budManifest)
    {
        if (string.IsNullOrWhiteSpace(budManifest.AssemblyName))
            throw new Exception($"{nameof(budManifest.AssemblyName)} is not specified");
        if (string.IsNullOrWhiteSpace(budManifest.BudId))
            throw new Exception($"{nameof(budManifest.BudId)} is not specified");
        if (string.IsNullOrWhiteSpace(budManifest.BudName))
            throw new Exception($"{nameof(budManifest.BudName)} is not specified");
        if (string.IsNullOrWhiteSpace(budManifest.BudAuthor))
            throw new Exception($"{nameof(budManifest.BudAuthor)} is not specified");
        if (budManifest.BudVersion is null)
            throw new Exception($"{nameof(budManifest.BudVersion)} is null");
        if (budManifest.BudDependencies.Any(d => string.IsNullOrWhiteSpace(d.BudId)))
            throw new Exception("At least one dependency has an unspecified bud ID");
        if (budManifest.BudIncompatibilities.Any(d => string.IsNullOrWhiteSpace(d.BudId)))
            throw new Exception("At least one incompatibility has an unspecified bud ID");
        if (budManifest.BudDependencies.Any(d => d.BudId == budManifest.BudId))
            throw new Exception("The bud cannot have a dependency with itself");
        if (budManifest.BudIncompatibilities.Any(d => d.BudId == budManifest.BudId))
            throw new Exception("The bud cannot have an incompatibility with itself");

        foreach (BudDependency budDependency in budManifest.BudDependencies)
        {
            if (budDependency.Version is null)
            {
                throw new Exception(
                    $"The dependency {nameof(budDependency.BudId)} has a null {nameof(budDependency.Version)}");
            }
        }
    }

    private bool TryValidateBudAssembly(string budAssemblyPath, [NotNullWhen(true)] out TypeDefinition? budType)
    {
        budType = null;

        try
        {
            ModuleDefinition moduleDefinition = ModuleDefinition.FromFile(budAssemblyPath);
            List<TypeDefinition> iBuds = moduleDefinition.GetAllTypes()
                .Where(x => !x.IsAbstract
                            && x.BaseType?.FullName == typeof(Bud).FullName!
                            && x.GetConstructor() is not null)
                .ToList();

            switch (iBuds.Count)
            {
                case <= 0:
                    throw new Exception(
                        $"There are no non abstract classes in the assembly that inherits from {nameof(Bud)} with a parameterless constructor");
                case > 1:
                    throw new Exception(
                        $"There are more than 1 non abstract classes in the assembly that inherits from {nameof(Bud)} with a parameterless constructor " +
                        $"which creates ambiguity. Here are the list of types:\n\n{string.Join("\n", iBuds.Select(x => x.FullName))}");
                default:
                    budType = iBuds.Single();
                    return true;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "The bud assembly {budAssemblyPath} could not be loaded", budAssemblyPath);
            return false;
        }
    }
}