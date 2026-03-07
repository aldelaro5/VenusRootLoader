using VenusRootLoader.Api.Leaves;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global

namespace VenusRootLoader.Api;

public partial class Venus
{
    public ItemLeaf RegisterItem(string namedId) =>
        RegistryResolver.Resolve<ItemLeaf>().RegisterNew(namedId, BudId);

    public ItemLeaf GetItem(string namedId) =>
        RegistryResolver.Resolve<ItemLeaf>().Get(namedId);

    public IReadOnlyCollection<ItemLeaf> GetAllItems() =>
        RegistryResolver.Resolve<ItemLeaf>().GetAll();

    public MedalLeaf RegisterMedal(string namedId, MainManager.BadgeTypes? orderAfter, int orderPriority) =>
        RegistryResolver.ResolveWithOrdering<MedalLeaf>().RegisterNewWithOrdering(
            namedId,
            BudId,
            (int?)orderAfter,
            orderPriority);

    public MedalLeaf GetMedal(string namedId) =>
        RegistryResolver.Resolve<MedalLeaf>().Get(namedId);

    public IReadOnlyCollection<MedalLeaf> GetAllMedals() =>
        RegistryResolver.Resolve<MedalLeaf>().GetAll();

    public TermacadePrizeLeaf RegisterTermacadePrize(string namedId) =>
        RegistryResolver.Resolve<TermacadePrizeLeaf>().RegisterNew(namedId, BudId);

    public TermacadePrizeLeaf GetTermacadePrize(string namedId) =>
        RegistryResolver.Resolve<TermacadePrizeLeaf>().Get(namedId);

    public IReadOnlyCollection<TermacadePrizeLeaf> GetAllTermacadePrizes() =>
        RegistryResolver.Resolve<TermacadePrizeLeaf>().GetAll();

    public FlagLeaf RegisterFlag(string namedId) =>
        RegistryResolver.Resolve<FlagLeaf>().RegisterNew(namedId, BudId);

    public FlagLeaf GetFlag(string namedId) =>
        RegistryResolver.Resolve<FlagLeaf>().Get(namedId);

    public IReadOnlyCollection<FlagLeaf> GetAllFlags() =>
        RegistryResolver.Resolve<FlagLeaf>().GetAll();

    public FlagvarLeaf RegisterFlagvar(string namedId) =>
        RegistryResolver.Resolve<FlagvarLeaf>().RegisterNew(namedId, BudId);

    public FlagvarLeaf GetFlagvar(string namedId) =>
        RegistryResolver.Resolve<FlagvarLeaf>().Get(namedId);

    public IReadOnlyCollection<FlagvarLeaf> GetAllFlagvars() =>
        RegistryResolver.Resolve<FlagvarLeaf>().GetAll();

    public FlagstringLeaf RegisterFlagstring(string namedId) =>
        RegistryResolver.Resolve<FlagstringLeaf>().RegisterNew(namedId, BudId);

    public FlagstringLeaf GetFlagstring(string namedId) =>
        RegistryResolver.Resolve<FlagstringLeaf>().Get(namedId);

    public IReadOnlyCollection<FlagstringLeaf> GetAllFlagstrings() =>
        RegistryResolver.Resolve<FlagstringLeaf>().GetAll();

    public PrizeMedalLeaf RegisterPrizeMedal(string namedId) =>
        RegistryResolver.Resolve<PrizeMedalLeaf>().RegisterNew(namedId, BudId);

    public PrizeMedalLeaf GetPrizeMedal(string namedId) =>
        RegistryResolver.Resolve<PrizeMedalLeaf>().Get(namedId);

    public IReadOnlyCollection<PrizeMedalLeaf> GetAllPrizeMedals() =>
        RegistryResolver.Resolve<PrizeMedalLeaf>().GetAll();

    public DiscoveryLeaf RegisterDiscovery(string namedId, int? orderAfter, int orderPriority) =>
        RegistryResolver.ResolveWithOrdering<DiscoveryLeaf>().RegisterNewWithOrdering(
            namedId,
            BudId,
            orderAfter,
            orderPriority);

    public DiscoveryLeaf GetDiscovery(string namedId) =>
        RegistryResolver.Resolve<DiscoveryLeaf>().Get(namedId);

    public IReadOnlyCollection<DiscoveryLeaf> GetAllDiscoveries() =>
        RegistryResolver.Resolve<DiscoveryLeaf>().GetAll();

    public RecordLeaf RegisterRecord(string namedId, int? orderAfter, int orderPriority) =>
        RegistryResolver.ResolveWithOrdering<RecordLeaf>().RegisterNewWithOrdering(
            namedId,
            BudId,
            orderAfter,
            orderPriority);

    public RecordLeaf GetRecord(string namedId) =>
        RegistryResolver.Resolve<RecordLeaf>().Get(namedId);

    public IReadOnlyCollection<RecordLeaf> GetAllRecords() =>
        RegistryResolver.Resolve<RecordLeaf>().GetAll();

    public MenuTextLeaf RegisterMenuText(string namedId) =>
        RegistryResolver.Resolve<MenuTextLeaf>().RegisterNew(namedId, BudId);

    public MenuTextLeaf GetMenuText(string namedId) =>
        RegistryResolver.Resolve<MenuTextLeaf>().Get(namedId);

    public IReadOnlyCollection<MenuTextLeaf> GetAllMenuTexts() =>
        RegistryResolver.Resolve<MenuTextLeaf>().GetAll();

    public CommonDialogueLeaf RegisterCommonDialogue(string namedId) =>
        RegistryResolver.Resolve<CommonDialogueLeaf>().RegisterNew(namedId, BudId);

    public CommonDialogueLeaf GetCommonDialogue(string namedId) =>
        RegistryResolver.Resolve<CommonDialogueLeaf>().Get(namedId);

    public IReadOnlyCollection<CommonDialogueLeaf> GetAllCommonDialogues() =>
        RegistryResolver.Resolve<CommonDialogueLeaf>().GetAll();

    public CrystalBerryLeaf RegisterCrystalBerry(string namedId) =>
        RegistryResolver.Resolve<CrystalBerryLeaf>().RegisterNew(namedId, BudId);

    public CrystalBerryLeaf GetCrystalBerry(string namedId) =>
        RegistryResolver.Resolve<CrystalBerryLeaf>().Get(namedId);

    public IReadOnlyCollection<CrystalBerryLeaf> GetAllCrystalBerries() =>
        RegistryResolver.Resolve<CrystalBerryLeaf>().GetAll();

    public RecipeLeaf RegisterRecipe(string namedId) =>
        RegistryResolver.Resolve<RecipeLeaf>().RegisterNew(namedId, BudId);

    public RecipeLeaf GetRecipe(string namedId) =>
        RegistryResolver.Resolve<RecipeLeaf>().Get(namedId);

    public IReadOnlyCollection<RecipeLeaf> GetAllRecipes() =>
        RegistryResolver.Resolve<RecipeLeaf>().GetAll();

    public RecipeLibraryEntryLeaf RegisterRecipeLibraryEntry(string namedId) =>
        RegistryResolver.Resolve<RecipeLibraryEntryLeaf>().RegisterNew(namedId, BudId);

    public RecipeLibraryEntryLeaf GetRecipeLibraryEntry(string namedId) =>
        RegistryResolver.Resolve<RecipeLibraryEntryLeaf>().Get(namedId);

    public IReadOnlyCollection<RecipeLibraryEntryLeaf> GetAllRecipeLibraryEntries() =>
        RegistryResolver.Resolve<RecipeLibraryEntryLeaf>().GetAll();

    public AreaLeaf RegisterArea(string namedId) =>
        RegistryResolver.Resolve<AreaLeaf>().RegisterNew(namedId, BudId);

    public AreaLeaf GetArea(string namedId) =>
        RegistryResolver.Resolve<AreaLeaf>().Get(namedId);

    public IReadOnlyCollection<AreaLeaf> GetAllAreas() =>
        RegistryResolver.Resolve<AreaLeaf>().GetAll();

    public EnemyLeaf RegisterSpyableEnemy(
        string namedId,
        MainManager.Enemies? orderAfterInBestiary,
        int orderPriorityInBestiary)
    {
        EnemyLeaf enemyLeaf = RegistryResolver.ResolveWithOrdering<EnemyLeaf>().RegisterNewWithOrdering(
            namedId,
            BudId,
            (int?)orderAfterInBestiary,
            orderPriorityInBestiary);
        enemyLeaf.CanBeSpied = true;
        return enemyLeaf;
    }

    public EnemyLeaf RegisterNonSpyableEnemy(string namedId)
    {
        EnemyLeaf enemyLeaf = RegistryResolver.Resolve<EnemyLeaf>().RegisterNew(namedId, BudId);
        enemyLeaf.CanBeSpied = false;
        return enemyLeaf;
    }

    public EnemyLeaf GetEnemy(string namedId) =>
        RegistryResolver.Resolve<EnemyLeaf>().Get(namedId);

    public IReadOnlyCollection<EnemyLeaf> GetAllEnemies() =>
        RegistryResolver.Resolve<EnemyLeaf>().GetAll();

    public MedalFortuneTellerHintLeaf RegisterMedalFortuneTellerHint(string namedId) =>
        RegistryResolver.Resolve<MedalFortuneTellerHintLeaf>().RegisterNew(namedId, BudId);

    public MedalFortuneTellerHintLeaf GetMedalFortuneTellerHint(string namedId) =>
        RegistryResolver.Resolve<MedalFortuneTellerHintLeaf>().Get(namedId);

    public IReadOnlyCollection<MedalFortuneTellerHintLeaf> GetAllMedalFortuneTellerHints() =>
        RegistryResolver.Resolve<MedalFortuneTellerHintLeaf>().GetAll();

    public DialogueBleepLeaf RegisterDialogueBleep(string namedId) =>
        RegistryResolver.Resolve<DialogueBleepLeaf>().RegisterNew(namedId, BudId);

    public DialogueBleepLeaf GetDialogueBleep(string namedId) =>
        RegistryResolver.Resolve<DialogueBleepLeaf>().Get(namedId);

    public IReadOnlyCollection<DialogueBleepLeaf> GetAllDialogueBleeps() =>
        RegistryResolver.Resolve<DialogueBleepLeaf>().GetAll();
}