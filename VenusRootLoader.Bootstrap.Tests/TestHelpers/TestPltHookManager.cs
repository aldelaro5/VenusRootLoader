using VenusRootLoader.Bootstrap.Shared;

namespace VenusRootLoader.Bootstrap.Tests.TestHelpers;

public sealed class TestPltHookManager : IPltHooksManager
{
    internal Dictionary<(string fileName, string functionName), Delegate> Hooks { get; } = new();

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
        return Hooks.TryGetValue((fileName, functionName), out var hook)
            ? hook.DynamicInvoke(args)
            : null;
    }
}