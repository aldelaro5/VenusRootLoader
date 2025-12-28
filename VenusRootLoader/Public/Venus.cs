using VenusRootLoader.VenusInternals;

// ReSharper disable UnusedMember.Global

namespace VenusRootLoader.Public;

public sealed partial class Venus
{
    private readonly VenusServices _venusServices;
    private readonly string _budId;
    private bool _hasGlobalBehaviour;

    internal Venus(string budId, VenusServices venusServices)
    {
        _budId = budId;
        _venusServices = venusServices;
    }
}