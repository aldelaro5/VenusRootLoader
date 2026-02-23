using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Registry;

internal sealed class DiscoveriesRegistry : AutoIncrementBasedRegistry<DiscoveryLeaf>
{
    public DiscoveriesRegistry(ILogger<DiscoveriesRegistry> logger) : base(logger) { }
}