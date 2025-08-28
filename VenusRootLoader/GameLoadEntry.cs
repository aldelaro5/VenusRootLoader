namespace VenusRootLoader;

public static class GameLoadEntry
{
    // private static UnityExplorer.ExplorerStandalone _explorerInstance;
    public static void Main()
    {
        Console.WriteLine("Hello in the game load entry!");
        Console.WriteLine("Loaded assemblies:");
        foreach (var ass in AppDomain.CurrentDomain.GetAssemblies())
        {
            Console.WriteLine($"\t{ass.GetName().FullName}");
        }

        Console.WriteLine("Loading UnityExplorer...");
        // _explorerInstance = UnityExplorer.ExplorerStandalone.CreateInstance();
    }
}