using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Registry;

internal sealed class CrystalBerriesRegistry : AutoSequentialIdBasedRegistry<CrystalBerryLeaf>
{
    public CrystalBerriesRegistry(ILogger<CrystalBerriesRegistry> logger)
        : base(logger, IdSequenceDirection.Increment)
    {
    }
}