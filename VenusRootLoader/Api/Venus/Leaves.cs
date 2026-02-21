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

    public MedalLeaf RegisterMedal(string namedId, MainManager.BadgeTypes? orderAfter, int orderPriority) =>
        _registryResolver.ResolveWithOrdering<MedalLeaf>().RegisterNewWithOrdering(
            namedId,
            _budId,
            (int?)orderAfter,
            orderPriority);

    public MedalLeaf GetMedal(string namedId) =>
        _registryResolver.Resolve<MedalLeaf>().Get(namedId);

    public IReadOnlyCollection<MedalLeaf> GetAllMedals() =>
        _registryResolver.Resolve<MedalLeaf>().GetAll();

    public RecipeLeaf RegisterRecipe(string namedId) =>
        _registryResolver.Resolve<RecipeLeaf>().RegisterNew(namedId, _budId);

    public RecipeLeaf GetRecipe(string namedId) =>
        _registryResolver.Resolve<RecipeLeaf>().Get(namedId);

    public IReadOnlyCollection<RecipeLeaf> GetAllRecipes() =>
        _registryResolver.Resolve<RecipeLeaf>().GetAll();

    public TermacadePrizeLeaf RegisterTermacadePrize(string namedId) =>
        _registryResolver.Resolve<TermacadePrizeLeaf>().RegisterNew(namedId, _budId);

    public TermacadePrizeLeaf GetTermacadePrize(string namedId) =>
        _registryResolver.Resolve<TermacadePrizeLeaf>().Get(namedId);

    public IReadOnlyCollection<TermacadePrizeLeaf> GetAllTermacadePrizes() =>
        _registryResolver.Resolve<TermacadePrizeLeaf>().GetAll();

    public FlagLeaf RegisterFlag(string namedId) =>
        _registryResolver.Resolve<FlagLeaf>().RegisterNew(namedId, _budId);

    public FlagLeaf GetFlag(string namedId) =>
        _registryResolver.Resolve<FlagLeaf>().Get(namedId);

    public IReadOnlyCollection<FlagLeaf> GetAllFlags() =>
        _registryResolver.Resolve<FlagLeaf>().GetAll();

    public FlagvarLeaf RegisterFlagvar(string namedId) =>
        _registryResolver.Resolve<FlagvarLeaf>().RegisterNew(namedId, _budId);

    public FlagvarLeaf GetFlagvar(string namedId) =>
        _registryResolver.Resolve<FlagvarLeaf>().Get(namedId);

    public IReadOnlyCollection<FlagvarLeaf> GetAllFlagvars() =>
        _registryResolver.Resolve<FlagvarLeaf>().GetAll();
}