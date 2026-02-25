using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Registry;

internal sealed class CommonDialoguesRegistry : AutoSequentialIdBasedRegistry<CommonDialogueLeaf>
{
    public CommonDialoguesRegistry(ILogger<CommonDialoguesRegistry> logger)
        : base(logger, IdSequenceDirection.Decrement, -1)
    {
    }
}