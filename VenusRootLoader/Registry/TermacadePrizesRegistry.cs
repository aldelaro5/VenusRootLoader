using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Registry;

internal sealed class TermacadePrizesRegistry : AutoIncrementBasedRegistry<TermacadePrizeLeaf>
{
    public TermacadePrizesRegistry(ILogger<TermacadePrizesRegistry> logger) : base(logger) { }
}