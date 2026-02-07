using VenusRootLoader.Api.Leaves;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global

namespace VenusRootLoader.Api;

public partial class Venus
{
    public ItemLeaf RegisterItem(string namedId) =>
        _registryResolver.Resolve<ItemLeaf>().RegisterNew(namedId, _budId, null, 0);

    public ItemLeaf GetItem(string namedId) =>
        _registryResolver.Resolve<ItemLeaf>().Get(namedId);

    public IReadOnlyCollection<ItemLeaf> GetAllItems() =>
        _registryResolver.Resolve<ItemLeaf>().GetAll();

    public MedalLeaf RegisterMedal(string namedId, MainManager.BadgeTypes? orderAfter, int orderPriority) =>
        _registryResolver.Resolve<MedalLeaf>().RegisterNew(namedId, _budId, (int?)orderAfter, orderPriority);

    public MedalLeaf GetMedal(string namedId) =>
        _registryResolver.Resolve<MedalLeaf>().Get(namedId);

    public IReadOnlyCollection<MedalLeaf> GetAllMedals() =>
        _registryResolver.Resolve<MedalLeaf>().GetAll();

    public RecipeLeaf RegisterRecipe(string namedId) =>
        _registryResolver.Resolve<RecipeLeaf>().RegisterNew(namedId, _budId, null, 0);

    public RecipeLeaf GetRecipe(string namedId) =>
        _registryResolver.Resolve<RecipeLeaf>().Get(namedId);

    public IReadOnlyCollection<RecipeLeaf> GetAllRecipes() =>
        _registryResolver.Resolve<RecipeLeaf>().GetAll();
}