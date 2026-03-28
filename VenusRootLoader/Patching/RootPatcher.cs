namespace VenusRootLoader.Patching;

/// <summary>
/// A service that allows to tell all <see cref="ITopLevelPatcher"/> to executes their patching process. This is meant
/// to be used from <see cref="Entry"/> during boot.
/// </summary>
internal sealed class RootPatcher
{
    private readonly IEnumerable<ITopLevelPatcher> _patchers;

    public RootPatcher(IEnumerable<ITopLevelPatcher> patchers) => _patchers = patchers;

    public void RunAllTopLevelPatchers()
    {
        foreach (ITopLevelPatcher patcher in _patchers)
            patcher.Patch();
    }
}