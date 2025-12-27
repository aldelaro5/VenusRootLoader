using Microsoft.Extensions.Logging;
using VenusRootLoader.Binders;
using VenusRootLoader.Registry;
using VenusRootLoader.Unity;

namespace VenusRootLoader.Venus;

internal sealed class VenusServices
{
    internal required ContentBinder ContentBinder { get; init; }
    internal required GlobalContentRegistry GlobalContentRegistry { get; init; } 
    internal required GlobalMonoBehaviourExecution GlobalMonoBehaviourExecution { get; init; }
    internal required ILogger<Venus> Logger { get; init; }
}