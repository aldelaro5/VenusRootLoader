using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Registry;

internal sealed class RecipeLibraryEntriesRegistry : AutoSequentialIdBasedRegistry<RecipeLibraryEntryLeaf>
{
    public RecipeLibraryEntriesRegistry(ILogger<RecipeLibraryEntriesRegistry> logger)
        : base(logger, IdSequenceDirection.Increment)
    {
    }
}