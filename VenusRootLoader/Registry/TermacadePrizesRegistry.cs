using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Registry;

internal sealed class TermacadePrizesRegistry : AutoSequentialIdBasedRegistry<TermacadePrizeLeaf>
{
    public TermacadePrizesRegistry(ILogger<TermacadePrizesRegistry> logger)
        : base(logger, IdSequenceDirection.Increment)
    {
    }
}