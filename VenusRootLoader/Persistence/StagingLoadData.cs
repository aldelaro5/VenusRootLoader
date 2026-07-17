namespace VenusRootLoader.Persistence;

internal sealed class StagingLoadData
{
    public List<int> PartyOrder { get; } = new();
    public List<MainManager.BattleData> PlayerData { get; } = new();
    public int PartyLevel { get; set; }
    public int PartyExp { get; set; }
    public int NeededExp { get; set; }
    public int BaseTp { get; set; }
    public int Tp { get; set; }
    public int Money { get; set; }
    public int Bp { get; set; }
    public int MaxBp { get; set; }
    public int MaxItems { get; set; }
    public int MaxStorage { get; set; }
    public int ClockHour { get; set; }
    public int ClockMin { get; set; }
    public int ClockSec { get; set; }
    public int AreaId { get; set; }
    public List<List<int>> AvaliableBadgePool { get; } = new();
    public List<List<int>> BadgeShops { get; } = new();
    public List<List<int>> BoardQuests { get; } = new();
    public List<List<int>> Items { get; } = new();
    public List<int[]> Badges { get; } = new();
    public List<int[]> SamiraMusics { get; } = new();
    public List<int[]> StatBonus { get; } = new();
    public List<bool>[] LibraryStuff { get; } = [new(), new(), new(), new(), new()];
    public List<bool> Flags { get; } = new();
    public List<string> Flagstrings { get; } = new();
    public List<int> Flagvars { get; } = new();
    public List<bool> RegionalFlags { get; } = new();
    public List<bool> CrystalBerryFlags { get; } = new();
    public List<int> ExtraFollowers { get; } = new();
    public List<int[]> EnemyEncounter { get; } = new();

    internal void CommitToRuntimeState()
    {
        MainManager mainManager = MainManager.instance;
        mainManager.partyorder = PartyOrder.ToArray();
        mainManager.playerdata = PlayerData.ToArray();
        mainManager.partylevel = PartyLevel;
        mainManager.partyexp = PartyExp;
        mainManager.neededexp = NeededExp;
        mainManager.basetp = BaseTp;
        mainManager.tp = Tp;
        mainManager.money = Money;
        mainManager.bp = Bp;
        mainManager.maxbp = MaxBp;
        mainManager.maxitems = MaxItems;
        mainManager.maxstorage = MaxStorage;
        mainManager.clockhour = ClockHour;
        mainManager.clockmin = ClockMin;
        mainManager.clocksec = ClockSec;
        mainManager.areaid = AreaId;
        mainManager.avaliablebadgepool = AvaliableBadgePool.ToArray();
        mainManager.badgeshops = BadgeShops.ToArray();
        mainManager.boardquests = BoardQuests.ToArray();
        mainManager.items = Items.ToArray();
        mainManager.badges = Badges.ToList();
        mainManager.samiramusics = SamiraMusics.ToList();
        mainManager.statbonus = StatBonus.ToList();

        mainManager.librarystuff = new bool[LibraryStuff.Length, Math.Max(256, LibraryStuff.Max(x => x.Count))];
        for (int i = 0; i < LibraryStuff.Length; i++)
        for (int j = 0; j < LibraryStuff[i].Count; j++)
            mainManager.librarystuff[i, j] = LibraryStuff[i][j];

        mainManager.flags = Flags.ToArray();
        mainManager.flagstring = Flagstrings.ToArray();
        mainManager.flagvar = Flagvars.ToArray();
        mainManager.regionalflags = RegionalFlags.ToArray();
        mainManager.crystalbflags = CrystalBerryFlags.ToArray();
        mainManager.extrafollowers = ExtraFollowers.ToList();

        mainManager.enemyencounter = new int[Math.Max(256, EnemyEncounter.Count), 2];
        for (int i = 0; i < EnemyEncounter.Count; i++)
        for (int j = 0; j < EnemyEncounter[i].Length; j++)
            mainManager.enemyencounter[i, j] = EnemyEncounter[i][j];
    }
}