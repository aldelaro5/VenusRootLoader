using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Registry;

internal sealed class MenuTextsRegistry : AutoSequentialIdBasedRegistry<MenuTextLeaf>
{
    public MenuTextsRegistry(ILogger<MenuTextsRegistry> logger)
        : base(logger, IdSequenceDirection.Increment)
    {
    }
}