namespace VenusRootLoader.Persistence;

internal sealed class StagingLoadData
{
    internal List<int> PartyOrder { get; } = new();
    internal List<MainManager.BattleData> PlayerData { get; } = new();
    internal int PartyLevel { get; set; }
    internal int PartyExp { get; set; }
    internal int NeededExp { get; set; }
    internal int BaseTp { get; set; }
    internal int Tp { get; set; }
    internal int Money { get; set; }
    internal int Bp { get; set; }
    internal int MaxBp { get; set; }
    internal int MaxItems { get; set; }
    internal int MaxStorage { get; set; }
    internal int ClockHour { get; set; }
    internal int ClockMin { get; set; }
    internal int ClockSec { get; set; }
    internal int AreaId { get; set; }
    internal List<List<int>> AvaliableBadgePool { get; } = new();
    internal List<List<int>> BadgeShops { get; } = new();
    internal List<List<int>> BoardQuests { get; } = new();
    internal List<List<int>> Items { get; } = new();
    internal List<int[]> Badges { get; } = new();
    internal List<int[]> SamiraMusics { get; } = new();
    internal List<int[]> StatBonus { get; } = new();
    internal List<bool>[] LibraryStuff { get; } = [new(), new(), new(), new(), new()];
    internal List<bool> Flags { get; } = new();
    internal List<string> Flagstrings { get; } = new();
    internal List<int> Flagvars { get; } = new();
    internal List<bool> RegionalFlags { get; } = new();
    internal List<bool> CrystalBerryFlags { get; } = new();
    internal List<int> ExtraFollowers { get; } = new();
    internal List<int[]> EnemyEncounter { get; } = new();

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