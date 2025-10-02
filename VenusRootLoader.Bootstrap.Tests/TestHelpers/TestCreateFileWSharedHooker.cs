using Windows.Win32.Foundation;
using VenusRootLoader.Bootstrap.Shared;

namespace VenusRootLoader.Bootstrap.Tests.TestHelpers;

public class TestCreateFileWSharedHooker : ICreateFileWSharedHooker
{
    internal Dictionary<string, (Func<string, bool> Predicate, CreateFileWSharedHooker.CreateFileWHook Hook)> Hooks { get; } = new();

    public void RegisterHook(string name, Func<string, bool> predicate, CreateFileWSharedHooker.CreateFileWHook hook)
    {
        Hooks.Add(name, (predicate, hook));
    }

    public void UnregisterHook(string name)
    {
        Hooks.Remove(name);
    }

    public unsafe HANDLE? SimulateHook(PCWSTR fileName)
    {
        foreach (var hook in Hooks)
        {
            if (!hook.Value.Predicate(fileName.ToString()))
                continue;
            hook.Value.Hook(out var handle, fileName, 0u, default, null, default, default, default);
            return handle;
        }

        return null;
    }
}
