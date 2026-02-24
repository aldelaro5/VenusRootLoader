using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Registry;

internal sealed class RecordsRegistry : AutoSequentialIdBasedRegistry<RecordLeaf>
{
    public RecordsRegistry(ILogger<RecordsRegistry> logger)
        : base(logger, IdSequenceDirection.Increment)
    {
    }
}