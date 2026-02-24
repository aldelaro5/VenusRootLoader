using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Registry;

internal sealed class PrizeMedalsRegistry : AutoSequentialIdBasedRegistry<PrizeMedalLeaf>
{
    public PrizeMedalsRegistry(ILogger<PrizeMedalsRegistry> logger)
        : base(logger, IdSequenceDirection.Increment)
    {
    }
}