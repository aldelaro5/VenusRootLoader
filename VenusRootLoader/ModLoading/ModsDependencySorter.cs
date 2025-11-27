using Microsoft.Extensions.Logging;
using VenusRootLoader.Models;

namespace VenusRootLoader.ModLoading;

internal interface IModsDependencySorter
{
    IList<ModInfo> SortModsTopologicallyFromDependencyGraph(IDictionary<string, ModInfo> modsById);
}

internal sealed class ModsDependencySorter : IModsDependencySorter
{
    private readonly ILogger<ModsDependencySorter> _logger;

    public ModsDependencySorter(ILogger<ModsDependencySorter> logger)
    {
        _logger = logger;
    }

    public IList<ModInfo> SortModsTopologicallyFromDependencyGraph(IDictionary<string, ModInfo> modsById)
    {
        List<ModInfo> result = new();

        HashSet<ModInfo> arrivedBefore = new();
        HashSet<ModInfo> visitedMods = new();

        foreach (ModInfo? input in modsById.Values)
        {
            Stack<ModInfo> currentPath = new();
            if (VisitModInDependencyGraph(input, currentPath))
                continue;

            List<string> cyclicModIds = FindSmallestCyclicDependencies(currentPath);
            throw new Exception(
                $"Cyclic Dependency detected, it is not possible to load any mods until this is addressed:\n\n" +
                $"{string.Join(" ->\n", cyclicModIds)}\n");
        }

        _logger.LogDebug(
            $"No dependency cycles detected, here are the mods topologically sorted:\n\n" +
            $"{string.Join(" -> ", result.Select(x => x.ModManifest.ModId))}");

        return result;

        bool VisitModInDependencyGraph(ModInfo mod, Stack<ModInfo> currentPathInGraph)
        {
            currentPathInGraph.Push(mod);

            bool beenOnThisNodeBefore = !arrivedBefore.Add(mod);
            if (beenOnThisNodeBefore)
            {
                if (!visitedMods.Contains(mod))
                    return false;

                currentPathInGraph.Pop();
                return true;
            }

            IEnumerable<ModInfo?> dependencies = GetModDependenciesThatArePresent(mod, modsById);
            if (dependencies.Any(dependencyNode => !VisitModInDependencyGraph(dependencyNode!, currentPathInGraph)))
                return false;

            visitedMods.Add(mod);
            if (modsById.ContainsKey(mod.ModManifest.ModId))
                result.Add(mod);

            currentPathInGraph.Pop();
            return true;
        }
    }

    private static List<string> FindSmallestCyclicDependencies(Stack<ModInfo> fullCyclicPath)
    {
        HashSet<string> uniqueModsInCyclicPath = new(StringComparer.InvariantCulture);
        List<string> smallestCyclicPathFound = new();

        while (fullCyclicPath.Count > 0)
        {
            ModInfo modInCyclicPath = fullCyclicPath.Pop();
            smallestCyclicPathFound.Add(modInCyclicPath.ModManifest.ModId);
            if (!uniqueModsInCyclicPath.Add(modInCyclicPath.ModManifest.ModId))
                break;
        }

        smallestCyclicPathFound.Reverse();
        return smallestCyclicPathFound;
    }

    private static IEnumerable<ModInfo?> GetModDependenciesThatArePresent(
        ModInfo mod,
        IDictionary<string, ModInfo> modsById)
    {
        return mod.ModManifest.ModDependencies
            .Select(modDependency => modsById
                .TryGetValue(modDependency.ModId, out ModInfo? dependency)
                ? dependency
                : null)
            .Where(x => x is not null);
    }
}