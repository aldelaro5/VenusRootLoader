using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Registry;

internal sealed class FlagvarsRegistry : AutoSequentialIdBasedRegistry<FlagvarLeaf>
{
    public FlagvarsRegistry(ILogger<FlagvarsRegistry> logger)
        : base(logger, IdSequenceDirection.Increment)
    {
    }
}