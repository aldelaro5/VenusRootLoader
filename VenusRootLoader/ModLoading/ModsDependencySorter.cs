using VenusRootLoader.Models;

namespace VenusRootLoader.ModLoading;

internal interface IModsDependencySorter
{
    IList<ModInfo> DetermineModsLoadOrder(IList<ModInfo> mods);
}

internal sealed class ModsDependencySorter : IModsDependencySorter
{
    public IList<ModInfo> DetermineModsLoadOrder(IList<ModInfo> mods)
    {
        Dictionary<string, ModInfo> modsById = mods.ToDictionary(m => m.ModManifest.ModId);
        List<ModInfo> result = new();

        HashSet<ModInfo> arrivedBefore = new();
        HashSet<ModInfo> visited = new();

        foreach (ModInfo? input in modsById.Values)
        {
            Stack<ModInfo> currentPath = new();
            if (Visit(input, currentPath))
                continue;

            HashSet<string> uniqueNodesInCyclicPath = new(StringComparer.InvariantCulture);
            List<string> smallestCyclicPathFound = new();
            while (currentPath.Count > 0)
            {
                ModInfo nodeInCyclicPath = currentPath.Pop();
                smallestCyclicPathFound.Add(nodeInCyclicPath.ModManifest.ModId);
                if (!uniqueNodesInCyclicPath.Add(nodeInCyclicPath.ModManifest.ModId))
                    break;
            }

            smallestCyclicPathFound.Reverse();
            throw new Exception(
                $"Cyclic Dependency detected, it is not possible to load any mods until this is addressed:\n\n" +
                $"{string.Join(" ->\n", smallestCyclicPathFound)}\n");
        }

        return result
            .Where(mod => modsById.ContainsKey(mod.ModManifest.ModId))
            .ToList();

        bool Visit(ModInfo node, Stack<ModInfo> currentPath)
        {
            currentPath.Push(node);

            bool beenOnThisNodeBefore = !arrivedBefore.Add(node);
            if (beenOnThisNodeBefore)
            {
                if (!visited.Contains(node))
                    return false;

                currentPath.Pop();
                return true;
            }

            IEnumerable<ModInfo?> dependencies = GetModDependenciesThatArePresent(node, modsById);
            if (dependencies.Any(dependencyNode => !Visit(dependencyNode!, currentPath)))
                return false;

            visited.Add(node);
            result.Add(node);

            currentPath.Pop();
            return true;
        }
    }

    private static IEnumerable<ModInfo?> GetModDependenciesThatArePresent(
        ModInfo mod,
        Dictionary<string, ModInfo> metadata)
    {
        return mod.ModManifest.ModDependencies
            .Select(modDependency => metadata
                .TryGetValue(modDependency.ModId, out ModInfo? dependency)
                ? dependency
                : null)
            .Where(x => x is not null);
    }
}