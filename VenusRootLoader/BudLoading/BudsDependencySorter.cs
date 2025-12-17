using Microsoft.Extensions.Logging;
using VenusRootLoader.Models;

namespace VenusRootLoader.BudLoading;

internal interface IBudsDependencySorter
{
    IList<BudInfo> SortBudsTopologicallyFromDependencyGraph(IDictionary<string, BudInfo> budsById);
}

internal sealed class BudsDependencySorter : IBudsDependencySorter
{
    private readonly ILogger<BudsDependencySorter> _logger;

    public BudsDependencySorter(ILogger<BudsDependencySorter> logger)
    {
        _logger = logger;
    }

    public IList<BudInfo> SortBudsTopologicallyFromDependencyGraph(IDictionary<string, BudInfo> budsById)
    {
        List<BudInfo> result = new();

        HashSet<BudInfo> arrivedBefore = new();
        HashSet<BudInfo> visitedBuds = new();

        foreach (BudInfo? bud in budsById.Values)
        {
            Stack<BudInfo> currentPath = new();
            if (VisitBudInDependencyGraph(bud, currentPath))
                continue;

            List<string> cyclicModIds = FindSmallestCyclicDependencies(currentPath);
            throw new Exception(
                $"Cyclic Dependency detected, it is not possible to load any buds until this is addressed:\n\n" +
                $"{string.Join(" ->\n", cyclicModIds)}\n");
        }

        _logger.LogDebug(
            $"No dependency cycles detected, here are the buds topologically sorted:\n\n" +
            $"{string.Join(" -> ", result.Select(x => x.BudManifest.BudId))}");

        return result;

        bool VisitBudInDependencyGraph(BudInfo bud, Stack<BudInfo> currentPathInGraph)
        {
            currentPathInGraph.Push(bud);

            bool beenOnThisNodeBefore = !arrivedBefore.Add(bud);
            if (beenOnThisNodeBefore)
            {
                if (!visitedBuds.Contains(bud))
                    return false;

                currentPathInGraph.Pop();
                return true;
            }

            IEnumerable<BudInfo?> dependencies = GetBudDependenciesThatArePresent(bud, budsById);
            if (dependencies.Any(dependencyNode => !VisitBudInDependencyGraph(dependencyNode!, currentPathInGraph)))
                return false;

            visitedBuds.Add(bud);
            if (budsById.ContainsKey(bud.BudManifest.BudId))
                result.Add(bud);

            currentPathInGraph.Pop();
            return true;
        }
    }

    private static List<string> FindSmallestCyclicDependencies(Stack<BudInfo> fullCyclicPath)
    {
        HashSet<string> uniqueBudsInCyclicPath = new(StringComparer.InvariantCulture);
        List<string> smallestCyclicPathFound = new();

        while (fullCyclicPath.Count > 0)
        {
            BudInfo budInCyclicPath = fullCyclicPath.Pop();
            smallestCyclicPathFound.Add(budInCyclicPath.BudManifest.BudId);
            if (!uniqueBudsInCyclicPath.Add(budInCyclicPath.BudManifest.BudId))
                break;
        }

        smallestCyclicPathFound.Reverse();
        return smallestCyclicPathFound;
    }

    private static IEnumerable<BudInfo?> GetBudDependenciesThatArePresent(
        BudInfo bud,
        IDictionary<string, BudInfo> budsById)
    {
        return bud.BudManifest.BudDependencies
            .Select(budDependency => budsById
                .TryGetValue(budDependency.BudId, out BudInfo? dependency)
                ? dependency
                : null)
            .Where(x => x is not null);
    }
}