using Microsoft.Extensions.Logging;
using VenusRootLoader.Models;

namespace VenusRootLoader.ModLoading;

internal interface IModsValidator
{
    IDictionary<string, ModInfo> RemoveInvalidMods(IList<ModInfo> mods);
}

internal sealed class ModsValidator : IModsValidator
{
    private readonly ILogger<ModsValidator> _logger;

    public ModsValidator(ILogger<ModsValidator> logger)
    {
        _logger = logger;
    }

    public IDictionary<string, ModInfo> RemoveInvalidMods(IList<ModInfo> mods)
    {
        List<IGrouping<string, ModInfo>> modsGroupedById = mods
            .OrderByDescending(m => m.ModManifest.ModVersion)
            .GroupBy(m => m.ModManifest.ModId)
            .ToList();
        List<IGrouping<string, ModInfo>> duplicateMods = modsGroupedById
            .Where(g => g.Count() > 1)
            .ToList();
        Dictionary<string, ModInfo> uniqueMods = modsGroupedById
            .Except(duplicateMods)
            .ToDictionary(g => g.Key, g => g.Single());

        foreach (IGrouping<string, ModInfo> duplicateModGroup in duplicateMods)
        {
            ModInfo instanceChosenToLoad = duplicateModGroup.First();
            List<ModInfo> skippedInstances = duplicateModGroup.Skip(1).ToList();

            string chosenModInfo = $"{instanceChosenToLoad.ModManifest.ModVersion} - " +
                                   $"{instanceChosenToLoad.ModAssemblyPath} - " +
                                   $"Will be selected";
            IEnumerable<string> skippedModsInfo = skippedInstances.Select(mod =>
                $"{mod.ModManifest.ModVersion} - " +
                $"{mod.ModAssemblyPath} - " +
                $"Will be ignored");

            _logger.LogWarning(
                "The mod {modId} was found multiple times so the one with the highest version will be " +
                "selected and the rest will be ignored. Here are the versions and assemblies paths of the " +
                "instances found:\n\n{chosenMod}\n{SkippedMods}",
                duplicateModGroup.Key,
                chosenModInfo,
                string.Join("\n", skippedModsInfo)
            );

            uniqueMods.Add(duplicateModGroup.Key, instanceChosenToLoad);
        }

        var incompatibleModsById = uniqueMods
            .Select(mod => (
                modId: mod.Key,
                incompatibilities: mod.Value.ModManifest.ModIncompatibilities
                    .Where(incompatibility => uniqueMods.ContainsKey(incompatibility.ModId)))
            )
            .Where(mod => mod.incompatibilities.Any())
            .ToList();

        foreach (var mod in incompatibleModsById)
        {
            IEnumerable<string> incompatibilitiesInfo = mod.incompatibilities.Select(i => i.ModId);
            _logger.LogError(
                "The mod {modId} will not be loaded because it is incompatible with the following mods " +
                "which are present:\n\n{incompatibilities}",
                mod.modId,
                string.Join("\n", incompatibilitiesInfo));

            uniqueMods.Remove(mod.modId);
        }

        return uniqueMods;
    }
}