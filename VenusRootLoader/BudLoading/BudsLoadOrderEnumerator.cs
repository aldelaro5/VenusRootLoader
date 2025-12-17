using Microsoft.Extensions.Logging;
using NuGet.Versioning;
using VenusRootLoader.Models;

namespace VenusRootLoader.BudLoading;

internal interface IBudsLoadOrderEnumerator
{
    IEnumerable<BudInfo> EnumerateBudsWithFulfilledDependencies(IEnumerable<BudInfo> sortedBuds);
    void MarkBudAsFailedDuringLoad(BudInfo bud);
}

internal sealed class BudsLoadOrderEnumerator : IBudsLoadOrderEnumerator
{
    private readonly ILogger<BudsLoadOrderEnumerator> _logger;

    private record BudDependencyErrorInfo
    {
        public required string BudId { get; init; }
        public required bool Optional { get; init; }
        public required string Reason { get; init; }
    }

    private readonly List<string> _failedBudsDuringLoad = [];
    private readonly Dictionary<string, NuGetVersion> _budIdsWithFulfilledDependencies = new(StringComparer.Ordinal);
    private readonly HashSet<string> _budIdsWithUnfulfilledRequiredDependencies = new(StringComparer.Ordinal);

    public BudsLoadOrderEnumerator(ILogger<BudsLoadOrderEnumerator> logger)
    {
        _logger = logger;
    }

    public IEnumerable<BudInfo> EnumerateBudsWithFulfilledDependencies(IEnumerable<BudInfo> sortedBuds)
    {
        foreach (BudInfo budInfo in sortedBuds)
        {
            List<BudDependencyErrorInfo> dependenciesErrors = GetDependenciesErrorsForBud(budInfo);

            if (dependenciesErrors.Count > 0)
            {
                if (!HandleUnsatisfiedDependencies(dependenciesErrors, budInfo))
                {
                    _budIdsWithUnfulfilledRequiredDependencies.Add(budInfo.BudManifest.BudId);
                    continue;
                }
            }

            _budIdsWithFulfilledDependencies.Add(budInfo.BudManifest.BudId, budInfo.BudManifest.BudVersion);
            yield return budInfo;
        }
    }

    public void MarkBudAsFailedDuringLoad(BudInfo bud) => _failedBudsDuringLoad.Add(bud.BudManifest.BudId);

    private List<BudDependencyErrorInfo> GetDependenciesErrorsForBud(BudInfo budLoadingInfo)
    {
        List<BudDependencyErrorInfo> dependenciesErrors = new();

        foreach (BudDependency dependency in budLoadingInfo.BudManifest.BudDependencies)
        {
            bool isMissing = !_budIdsWithFulfilledDependencies.TryGetValue(
                dependency.BudId,
                out NuGetVersion dependencyVersion);
            if (isMissing)
            {
                dependenciesErrors.Add(
                    new()
                    {
                        BudId = dependency.BudId,
                        Optional = dependency.Optional,
                        Reason = _budIdsWithUnfulfilledRequiredDependencies.Contains(dependency.BudId)
                            ? "This bud has unsatisfied dependencies"
                            : "This bud is missing"
                    });
            }
            else if (_failedBudsDuringLoad.Contains(dependency.BudId))
            {
                dependenciesErrors.Add(
                    new()
                    {
                        BudId = dependency.BudId,
                        Optional = dependency.Optional,
                        Reason = "This bud threw an exception as it was being loaded"
                    });
            }
            else if (!dependency.Version.Satisfies(dependencyVersion!))
            {
                dependenciesErrors.Add(
                    new()
                    {
                        BudId = dependency.BudId,
                        Optional = dependency.Optional,
                        Reason =
                            $"This bud loaded successfully with a version of {dependencyVersion!.ToFullString()} " +
                            $"which does not satisfy the desired version range of {dependency.Version.ToNormalizedString()}"
                    });
            }
        }

        return dependenciesErrors;
    }

    private bool HandleUnsatisfiedDependencies(
        List<BudDependencyErrorInfo> dependencyErrors,
        BudInfo budLoadingInfo)
    {
        bool allDependenciesAreOptional = dependencyErrors.All(d => d.Optional);
        if (allDependenciesAreOptional)
        {
            _logger.LogWarning(
                "{bud} will be loaded, but it has unsatisfied optional dependencies:\n\n{errors}",
                budLoadingInfo.BudManifest.BudId,
                FormatDependenciesErrors(dependencyErrors));
            return true;
        }

        _logger.LogError(
            "{bud} will not be loaded because it has unsatisfied dependencies of which at least 1 was not optional:\n\n{errors}",
            budLoadingInfo.BudManifest.BudId,
            FormatDependenciesErrors(dependencyErrors));
        return false;
    }

    private static string FormatDependenciesErrors(List<BudDependencyErrorInfo> dependencyErrors)
    {
        IEnumerable<string> errors = dependencyErrors
            .Select(d => $"{d.BudId} - " +
                         $"{(d.Optional ? "Optional" : "Required")} - " +
                         $"{d.Reason}");
        return string.Join("\n", errors);
    }
}