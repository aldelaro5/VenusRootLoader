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

    public FlagstringLeaf RegisterFlagstring(string namedId) =>
        _registryResolver.Resolve<FlagstringLeaf>().RegisterNew(namedId, _budId);

    public FlagstringLeaf GetFlagstring(string namedId) =>
        _registryResolver.Resolve<FlagstringLeaf>().Get(namedId);

    public IReadOnlyCollection<FlagstringLeaf> GetAllFlagstrings() =>
        _registryResolver.Resolve<FlagstringLeaf>().GetAll();

    public PrizeMedalLeaf RegisterPrizeMedal(string namedId) =>
        _registryResolver.Resolve<PrizeMedalLeaf>().RegisterNew(namedId, _budId);

    public PrizeMedalLeaf GetPrizeMedal(string namedId) =>
        _registryResolver.Resolve<PrizeMedalLeaf>().Get(namedId);

    public IReadOnlyCollection<PrizeMedalLeaf> GetAllPrizeMedals() =>
        _registryResolver.Resolve<PrizeMedalLeaf>().GetAll();

    public DiscoveryLeaf RegisterDiscovery(string namedId, int? orderAfter, int orderPriority) =>
        _registryResolver.ResolveWithOrdering<DiscoveryLeaf>().RegisterNewWithOrdering(
            namedId,
            _budId,
            orderAfter,
            orderPriority);

    public DiscoveryLeaf GetDiscovery(string namedId) =>
        _registryResolver.Resolve<DiscoveryLeaf>().Get(namedId);

    public IReadOnlyCollection<DiscoveryLeaf> GetAllDiscoveries() =>
        _registryResolver.Resolve<DiscoveryLeaf>().GetAll();

    public RecordLeaf RegisterRecord(string namedId, int? orderAfter, int orderPriority) =>
        _registryResolver.ResolveWithOrdering<RecordLeaf>().RegisterNewWithOrdering(
            namedId,
            _budId,
            orderAfter,
            orderPriority);

    public RecordLeaf GetRecord(string namedId) =>
        _registryResolver.Resolve<RecordLeaf>().Get(namedId);

    public IReadOnlyCollection<RecordLeaf> GetAllRecords() =>
        _registryResolver.Resolve<RecordLeaf>().GetAll();

    public MenuTextLeaf RegisterMenuText(string namedId) =>
        _registryResolver.Resolve<MenuTextLeaf>().RegisterNew(namedId, _budId);

    public MenuTextLeaf GetMenuText(string namedId) =>
        _registryResolver.Resolve<MenuTextLeaf>().Get(namedId);

    public IReadOnlyCollection<MenuTextLeaf> GetAllMenuTexts() =>
        _registryResolver.Resolve<MenuTextLeaf>().GetAll();
}