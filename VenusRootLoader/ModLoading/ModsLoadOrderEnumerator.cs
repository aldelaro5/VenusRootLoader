using Microsoft.Extensions.Logging;
using VenusRootLoader.Models;

namespace VenusRootLoader.ModLoading;

internal interface IModsLoadOrderEnumerator
{
    IEnumerable<ModInfo> EnumerateModsWithFulfilledDependencies(IEnumerable<ModInfo> sortedMods);
    void MarkModAsFailed(ModInfo mod);
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

    private readonly List<string> _failedMods = [];
    private readonly HashSet<string> _modIdsWithSatisfiedDependencies = new(StringComparer.Ordinal);
    private readonly HashSet<string> _modIdsWithUnsatisfiedRequiredDependencies = new(StringComparer.Ordinal);

    public ModsLoadOrderEnumerator(ILogger<ModsLoadOrderEnumerator> logger)
    {
        _logger = logger;
    }

    public IEnumerable<ModInfo> EnumerateModsWithFulfilledDependencies(IEnumerable<ModInfo> sortedMods)
    {
        foreach (ModInfo modInfo in sortedMods)
        {
            List<ModDependencyErrorInfo> unsatisfiedDependencies =
                GetUnsatisfiedDependencies(
                    modInfo,
                    _modIdsWithSatisfiedDependencies,
                    _modIdsWithUnsatisfiedRequiredDependencies);

            if (unsatisfiedDependencies.Count > 0)
            {
                if (!HandleUnsatisfiedDependencies(unsatisfiedDependencies, modInfo))
                {
                    _modIdsWithUnsatisfiedRequiredDependencies.Add(modInfo.ModManifest.ModId);
                    continue;
                }
            }

            _modIdsWithSatisfiedDependencies.Add(modInfo.ModManifest.ModId);
            yield return modInfo;
        }
    }

    public void MarkModAsFailed(ModInfo mod) => _failedMods.Add(mod.ModManifest.ModId);

    private List<ModDependencyErrorInfo> GetUnsatisfiedDependencies(
        ModInfo modLoadingInfo,
        HashSet<string> modIdsWithSatisfiedDependencies,
        HashSet<string> modIdsWithUnsatisfiedRequiredDependencies)
    {
        List<ModDependencyErrorInfo> unsatisfiedDependencies = new();

        foreach (ModDependency dependency in modLoadingInfo.ModManifest.ModDependencies)
        {
            bool isMissing = !modIdsWithSatisfiedDependencies.Contains(dependency.ModId);
            if (isMissing)
            {
                unsatisfiedDependencies.Add(
                    new()
                    {
                        ModId = dependency.ModId,
                        Optional = dependency.Optional,
                        Reason = modIdsWithUnsatisfiedRequiredDependencies.Contains(dependency.ModId)
                            ? "This mod has unsatisfied dependencies"
                            : "This mod is missing"
                    });
            }
            else if (_failedMods.Contains(dependency.ModId))
            {
                unsatisfiedDependencies.Add(
                    new()
                    {
                        ModId = dependency.ModId,
                        Optional = dependency.Optional,
                        Reason = "This mod threw an exception as it was being loaded"
                    });
            }
        }

        return unsatisfiedDependencies;
    }

    private bool HandleUnsatisfiedDependencies(
        List<ModDependencyErrorInfo> unsatisfiedDependencies,
        ModInfo modLoadingInfo)
    {
        bool allOptionals = unsatisfiedDependencies.All(d => d.Optional);
        if (allOptionals)
        {
            _logger.LogWarning(
                "{Mod} will be loaded, but it has unsatisfied optional dependencies:\n\n{errors}",
                modLoadingInfo.ModManifest.ModId,
                FormatDependenciesErrors(unsatisfiedDependencies));
            return true;
        }

        _logger.LogError(
            "{Mod} will not be loaded because it has unsatisfied dependencies of which at least 1 was not optional:\n\n{errors}",
            modLoadingInfo.ModManifest.ModId,
            FormatDependenciesErrors(unsatisfiedDependencies));
        return false;
    }

    private static string FormatDependenciesErrors(List<ModDependencyErrorInfo> unsatisfiedDependencies)
    {
        IEnumerable<string> errors = unsatisfiedDependencies
            .Select(d => $"{d.ModId} - " +
                         $"{(d.Optional ? "Optional" : "Required")} - " +
                         $"{d.Reason}");
        return string.Join("\n", errors);
    }
}