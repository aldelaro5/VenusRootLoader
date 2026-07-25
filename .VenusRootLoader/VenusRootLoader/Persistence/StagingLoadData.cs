namespace VenusRootLoader.Persistence;

internal sealed class StagingLoadData
{
    public List<int> PartyOrder { get; } = new();
    public List<PartyMemberRuntimeState> PlayerData { get; } = new();
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

    internal void CommitToRuntimeState(IGameDataRuntimeState runtimeState)
    {
        runtimeState.PartyOrder = PartyOrder.ToArray();
        runtimeState.PlayerData = PlayerData;
        runtimeState.PartyLevel = PartyLevel;
        runtimeState.PartyExp = PartyExp;
        runtimeState.NeededExp = NeededExp;
        runtimeState.BaseTp = BaseTp;
        runtimeState.Tp = Tp;
        runtimeState.Money = Money;
        runtimeState.Bp = Bp;
        runtimeState.MaxBp = MaxBp;
        runtimeState.MaxItems = MaxItems;
        runtimeState.MaxStorage = MaxStorage;
        runtimeState.ClockHour = ClockHour;
        runtimeState.ClockMin = ClockMin;
        runtimeState.ClockSec = ClockSec;
        runtimeState.MapAreaId = AreaId;
        runtimeState.AvailableBadgePool = AvaliableBadgePool.ToArray();
        runtimeState.BadgeShops = BadgeShops.ToArray();
        runtimeState.BoardQuests = BoardQuests.ToArray();
        runtimeState.Items = Items.ToArray();
        runtimeState.Badges = Badges.ToList();
        runtimeState.SamiraMusics = SamiraMusics.ToList();
        runtimeState.StatBonus = StatBonus.ToList();

        runtimeState.LibraryStuff = new bool[LibraryStuff.Length, Math.Max(256, LibraryStuff.Max(x => x.Count))];
        for (int i = 0; i < LibraryStuff.Length; i++)
        for (int j = 0; j < LibraryStuff[i].Count; j++)
            runtimeState.LibraryStuff[i, j] = LibraryStuff[i][j];

        runtimeState.Flags = Flags.ToArray();
        runtimeState.Flagstring = Flagstrings.ToArray();
        runtimeState.Flagvar = Flagvars.ToArray();
        runtimeState.RegionalFlags = RegionalFlags.ToArray();
        runtimeState.CrystalBFlags = CrystalBerryFlags.ToArray();
        runtimeState.ExtraFollowers = ExtraFollowers.ToList();

        runtimeState.EnemyEncounter = new int[Math.Max(256, EnemyEncounter.Count), 2];
        for (int i = 0; i < EnemyEncounter.Count; i++)
        for (int j = 0; j < EnemyEncounter[i].Length; j++)
            runtimeState.EnemyEncounter[i, j] = EnemyEncounter[i][j];
    }
}