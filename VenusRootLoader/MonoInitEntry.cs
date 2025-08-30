namespace VenusRootLoader;

public class MonoInitEntry
{
    public static void Main()
    {
        Console.WriteLine("Hello from the Mono managed side");
        Console.WriteLine($"Environment.Version: {Environment.Version}");

        HarmonyLogger.Setup();
        GameLoadEntrypointInitializer.Setup();
    }
}