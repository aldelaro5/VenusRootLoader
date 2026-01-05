using VenusRootLoader.Api.Leaves;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global

namespace VenusRootLoader.Api;

public partial class Venus
{
    public ItemLeaf RegisterItem(string namedId) =>
        _registryResolver.Resolve<ItemLeaf>().RegisterNew(namedId, _budId);

    public ItemLeaf GetItem(string namedId) =>
        _registryResolver.Resolve<ItemLeaf>().Get(namedId);

    public IReadOnlyCollection<ItemLeaf> GetAllItems() =>
        _registryResolver.Resolve<ItemLeaf>().GetAll();

    public RecipeLeaf RegisterRecipe(string namedId) =>
        _registryResolver.Resolve<RecipeLeaf>().RegisterNew(namedId, _budId);

    public RecipeLeaf GetRecipe(string namedId) =>
        _registryResolver.Resolve<RecipeLeaf>().Get(namedId);

    public IReadOnlyCollection<RecipeLeaf> GetAllRecipes() =>
        _registryResolver.Resolve<RecipeLeaf>().GetAll();
}