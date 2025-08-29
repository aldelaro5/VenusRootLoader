using HarmonyLib;

namespace VenusRootLoader;

public class MonoInitEntry
{
    public static void Main()
    {
        Console.WriteLine("Hello from the Mono managed side");
        Console.WriteLine($"Environment.Version: {Environment.Version}");
        Console.WriteLine("Loaded assemblies:");
        foreach (var ass in AppDomain.CurrentDomain.GetAssemblies())
        {
            Console.WriteLine($"\t{ass.GetName().FullName}");
        }
        HarmonyLogger.Setup();
        GameLoadEntrypointInitializer.Setup();
    }
}