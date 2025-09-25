using VenusRootLoader.Bootstrap.Shared;

namespace VenusRootLoader.Bootstrap.Tests.TestHelpers;

public class TestPltHookManager : IPltHooksManager
{
    private Dictionary<(string fileName, string functionName), Delegate> Hooks { get; } = new();

    public void InstallHook<T>(string fileName, string functionName, T hook) where T : Delegate
    {
        Hooks[(fileName, functionName)] = hook;
    }

    public void UninstallHook(string fileName, string functionName)
    {
        Hooks.Remove((fileName, functionName));
    }

    public object? SimulateHook(string fileName, string functionName, params object?[]? args)
    {
        return Hooks[(fileName, functionName)].DynamicInvoke(args);
    }

    public bool ContainsHook(string fileName, string functionName)
    {
        return Hooks.TryGetValue((fileName, functionName), out _);
    }
}