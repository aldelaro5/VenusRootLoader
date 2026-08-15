using VenusRootLoader.BudLoading;

namespace VenusRootLoader.Patching.Logic;

/// <summary>
/// A patcher that loads all the buds and run their entry points. This patcher mainly exists to simplify the ordering
/// of patchers after loading all buds.
/// </summary>
internal sealed class BudLoaderTopLevelPatcher : ITopLevelPatcher
{
    private readonly IBudLoader _budLoader;

    public BudLoaderTopLevelPatcher(IBudLoader budLoader)
    {
        _budLoader = budLoader;
    }

    public void Patch() => _budLoader.LoadAllBuds();
}