using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Registry;

internal sealed class FlagsRegistry : AutoIncrementBasedRegistry<FlagLeaf>
{
    public FlagsRegistry(ILogger<FlagsRegistry> logger) : base(logger) { }
}