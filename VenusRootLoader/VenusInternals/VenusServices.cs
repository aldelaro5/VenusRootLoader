using Microsoft.Extensions.Logging;
using VenusRootLoader.Public;

namespace VenusRootLoader.VenusInternals;

internal sealed class VenusServices
{
    internal required ContentBinder ContentBinder { get; init; }
    internal required GlobalContentRegistry GlobalContentRegistry { get; init; } 
    internal required GlobalMonoBehaviourExecution GlobalMonoBehaviourExecution { get; init; }
    internal required ILogger<Venus> Logger { get; init; }
}