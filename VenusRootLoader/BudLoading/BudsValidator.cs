using Microsoft.Extensions.Logging;
using VenusRootLoader.Models;

namespace VenusRootLoader.BudLoading;

internal interface IBudsValidator
{
    IDictionary<string, BudInfo> RemoveInvalidBuds(IList<BudInfo> buds);
}

internal sealed class BudsValidator : IBudsValidator
{
    private readonly ILogger<BudsValidator> _logger;

    public BudsValidator(ILogger<BudsValidator> logger)
    {
        _logger = logger;
    }

    public IDictionary<string, BudInfo> RemoveInvalidBuds(IList<BudInfo> buds)
    {
        List<IGrouping<string, BudInfo>> budsGroupedById = buds
            .OrderByDescending(m => m.BudManifest.BudVersion)
            .GroupBy(m => m.BudManifest.BudId)
            .ToList();
        List<IGrouping<string, BudInfo>> duplicateBuds = budsGroupedById
            .Where(g => g.Count() > 1)
            .ToList();
        Dictionary<string, BudInfo> uniqueBuds = budsGroupedById
            .Except(duplicateBuds)
            .ToDictionary(g => g.Key, g => g.Single());

        foreach (IGrouping<string, BudInfo> duplicateBudGroup in duplicateBuds)
        {
            BudInfo instanceChosenToLoad = duplicateBudGroup.First();
            List<BudInfo> skippedInstances = duplicateBudGroup.Skip(1).ToList();

            string chosenBudInfo = $"{instanceChosenToLoad.BudManifest.BudVersion} - " +
                                   $"{instanceChosenToLoad.BudAssemblyPath} - " +
                                   $"Will be selected";
            IEnumerable<string> skippedBudsInfo = skippedInstances.Select(bud =>
                $"{bud.BudManifest.BudVersion} - " +
                $"{bud.BudAssemblyPath} - " +
                $"Will be ignored");

            _logger.LogWarning(
                "The bud {budId} was found multiple times so the one with the highest version will be " +
                "selected and the rest will be ignored. Here are the versions and assemblies paths of the " +
                "instances found:\n\n{chosenBud}\n{skippedBuds}",
                duplicateBudGroup.Key,
                chosenBudInfo,
                string.Join("\n", skippedBudsInfo)
            );

            uniqueBuds.Add(duplicateBudGroup.Key, instanceChosenToLoad);
        }

        var incompatibleBudsById = uniqueBuds
            .Select(bud => (
                budId: bud.Key,
                incompatibilities: bud.Value.BudManifest.BudIncompatibilities
                    .Where(incompatibility =>
                        uniqueBuds.TryGetValue(incompatibility.BudId, out BudInfo incompatibleBud)
                        && IsBudIncompatible(incompatibility, incompatibleBud))
                )
            )
            .Where(bud => bud.incompatibilities.Any())
            .ToList();

        foreach (var bud in incompatibleBudsById)
        {
            IEnumerable<string> incompatibilitiesInfo = bud.incompatibilities
                .Select(i => $"{i.BudId} version {uniqueBuds[i.BudId].BudManifest.BudVersion.ToFullString()}");
            _logger.LogError(
                "The bud {budId} will not be loaded because it is incompatible with the following buds " +
                "which are present:\n\n{incompatibilities}",
                bud.budId,
                string.Join("\n", incompatibilitiesInfo));

            uniqueBuds.Remove(bud.budId);
        }

        return uniqueBuds;
    }

    private static bool IsBudIncompatible(BudIncompatibility incompatibility, BudInfo incompatibleBud) =>
        incompatibility.Version is null ||
        incompatibility.Version.Satisfies(incompatibleBud.BudManifest.BudVersion);
}