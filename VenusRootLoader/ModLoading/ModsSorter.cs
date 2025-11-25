using VenusRootLoader.Models;

namespace VenusRootLoader.ModLoading;

internal interface IModsSorter
{
    IEnumerable<ModInfo> TopologicalSort(Dictionary<string, ModInfo> metadata);
}

internal class ModsSorter : IModsSorter
{
    public IEnumerable<ModInfo> TopologicalSort(Dictionary<string, ModInfo> metadata)
    {
        List<ModInfo> result = new();

        HashSet<ModInfo> arrivedBefore = new();
        HashSet<ModInfo> visited = new();

        foreach (ModInfo? input in metadata.Values)
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

        return result.Where(mod => metadata.ContainsKey(mod.ModManifest.ModId));

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

            IEnumerable<ModInfo?> dependencies = GetModDependenciesThatArePresent(node, metadata);
            if (dependencies.Any(dependencyNode => !Visit(dependencyNode!, currentPath)))
                return false;

            visited.Add(node);
            result.Add(node);

            currentPath.Pop();
            return true;
        }
    }

    private IEnumerable<ModInfo?> GetModDependenciesThatArePresent(ModInfo mod, Dictionary<string, ModInfo> metadata)
    {
        return mod.ModManifest.ModDependencies
            .Select(modDependency => metadata
                .TryGetValue(modDependency.ModId, out ModInfo? dependency)
                ? dependency
                : null)
            .Where(x => x is not null);
    }
}