using Microsoft.Extensions.Logging;
using VenusRootLoader.Models;

namespace VenusRootLoader.ModLoading;

internal interface IModsLoadOrderEnumerator
{
    IEnumerable<ModInfo> EnumerateModsWithFulfilledDependencies(IEnumerable<ModInfo> sortedMods);
    void MarkModAsFailedDuringLoad(ModInfo mod);
}

internal sealed class ModsLoadOrderEnumerator : IModsLoadOrderEnumerator
{
    private readonly ILogger<ModsLoadOrderEnumerator> _logger;

    private record ModDependencyErrorInfo
    {
        public required string ModId { get; init; }
        public required bool Optional { get; init; }
        public required string Reason { get; init; }
    }

    private readonly List<string> _failedModsDuringLoad = [];
    private readonly HashSet<string> _modIdsWithFulfilledDependencies = new(StringComparer.Ordinal);
    private readonly HashSet<string> _modIdsWithUnfulfilledRequiredDependencies = new(StringComparer.Ordinal);

    public ModsLoadOrderEnumerator(ILogger<ModsLoadOrderEnumerator> logger)
    {
        _logger = logger;
    }

    public IEnumerable<ModInfo> EnumerateModsWithFulfilledDependencies(IEnumerable<ModInfo> sortedMods)
    {
        foreach (ModInfo modInfo in sortedMods)
        {
            List<ModDependencyErrorInfo> dependenciesErrors = GetDependenciesErrorsForMod(modInfo);

            if (dependenciesErrors.Count > 0)
            {
                if (!HandleUnsatisfiedDependencies(dependenciesErrors, modInfo))
                {
                    _modIdsWithUnfulfilledRequiredDependencies.Add(modInfo.ModManifest.ModId);
                    continue;
                }
            }

            _modIdsWithFulfilledDependencies.Add(modInfo.ModManifest.ModId);
            yield return modInfo;
        }
    }

    public void MarkModAsFailedDuringLoad(ModInfo mod) => _failedModsDuringLoad.Add(mod.ModManifest.ModId);

    private List<ModDependencyErrorInfo> GetDependenciesErrorsForMod(ModInfo modLoadingInfo)
    {
        List<ModDependencyErrorInfo> dependenciesErrors = new();

        foreach (ModDependency dependency in modLoadingInfo.ModManifest.ModDependencies)
        {
            bool isMissing = !_modIdsWithFulfilledDependencies.Contains(dependency.ModId);
            if (isMissing)
            {
                dependenciesErrors.Add(
                    new()
                    {
                        ModId = dependency.ModId,
                        Optional = dependency.Optional,
                        Reason = _modIdsWithUnfulfilledRequiredDependencies.Contains(dependency.ModId)
                            ? "This mod has unsatisfied dependencies"
                            : "This mod is missing"
                    });
            }
            else if (_failedModsDuringLoad.Contains(dependency.ModId))
            {
                dependenciesErrors.Add(
                    new()
                    {
                        ModId = dependency.ModId,
                        Optional = dependency.Optional,
                        Reason = "This mod threw an exception as it was being loaded"
                    });
            }
        }

        return dependenciesErrors;
    }

    private bool HandleUnsatisfiedDependencies(
        List<ModDependencyErrorInfo> dependencyErrors,
        ModInfo modLoadingInfo)
    {
        bool allDependenciesAreOptional = dependencyErrors.All(d => d.Optional);
        if (allDependenciesAreOptional)
        {
            _logger.LogWarning(
                "{Mod} will be loaded, but it has unsatisfied optional dependencies:\n\n{errors}",
                modLoadingInfo.ModManifest.ModId,
                FormatDependenciesErrors(dependencyErrors));
            return true;
        }

        _logger.LogError(
            "{Mod} will not be loaded because it has unsatisfied dependencies of which at least 1 was not optional:\n\n{errors}",
            modLoadingInfo.ModManifest.ModId,
            FormatDependenciesErrors(dependencyErrors));
        return false;
    }

    private static string FormatDependenciesErrors(List<ModDependencyErrorInfo> dependencyErrors)
    {
        IEnumerable<string> errors = dependencyErrors
            .Select(d => $"{d.ModId} - " +
                         $"{(d.Optional ? "Optional" : "Required")} - " +
                         $"{d.Reason}");
        return string.Join("\n", errors);
    }
}