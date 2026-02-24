using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Registry;

internal sealed class FlagsRegistry : AutoSequentialIdBasedRegistry<FlagLeaf>
{
    public FlagsRegistry(ILogger<FlagsRegistry> logger)
        : base(logger, IdSequenceDirection.Increment)
    {
    }
}