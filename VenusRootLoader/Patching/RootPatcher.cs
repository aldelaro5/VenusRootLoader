namespace VenusRootLoader.Patching;

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