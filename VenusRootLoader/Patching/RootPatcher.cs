namespace VenusRootLoader.Patching;

/// <summary>
/// A service that allows to tell all <see cref="ITopLevelPatcher"/> to executes their patching process. This is meant
/// to be used from <see cref="Entry"/> during boot.
/// </summary>
internal sealed class RootPatcher
{
    private readonly IEnumerable<ITopLevelPatcher> _patchers;
    private readonly IHarmonyTypePatcher _harmonyTypePatcher;

    public RootPatcher(IEnumerable<ITopLevelPatcher> patchers, IHarmonyTypePatcher harmonyTypePatcher)
    {
        _patchers = patchers;
        _harmonyTypePatcher = harmonyTypePatcher;
    }

    public void RunAllTopLevelPatchers()
    {
        _harmonyTypePatcher.PatchAll(typeof(UnpatchedMethods));
        foreach (ITopLevelPatcher patcher in _patchers)
            patcher.Patch();
    }
}