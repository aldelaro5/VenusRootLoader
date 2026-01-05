using Microsoft.Extensions.Logging;
using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Registry;

internal sealed class RecipesRegistry : AutoIncrementBasedRegistry<RecipeLeaf>
{
    public RecipesRegistry(ILogger<RecipesRegistry> logger) : base(logger) { }
}