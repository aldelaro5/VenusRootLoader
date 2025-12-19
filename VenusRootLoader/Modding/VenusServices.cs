using Microsoft.Extensions.Logging;
using VenusRootLoader.Unity;

namespace VenusRootLoader.Modding;

internal sealed class VenusServices
{
    internal required GlobalMonoBehaviourExecution GlobalMonoBehaviourExecution { get; init; }
    internal required ILogger<Venus> Logger { get; init; }
}