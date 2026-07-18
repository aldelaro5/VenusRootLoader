using UnityEngine;

namespace VenusRootLoader.Persistence;

internal interface IGameDataRuntimeState
{
    Vector3 PlayerPosition { get; set; }
    int[] PartyOrder { get; set; }
    List<PartyMemberRuntimeState> PlayerData { get; set; }
    int MapAreaId { get; set; }
    string MapName { get; }
    int PartyLevel { get; set; }
    int PartyExp { get; set; }
    int NeededExp { get; set; }
    int BaseTp { get; set; }
    int Tp { get; set; }
    int Money { get; set; }
    int Bp { get; set; }
    int MaxBp { get; set; }
    int MaxItems { get; set; }
    int MaxStorage { get; set; }
    int ClockHour { get; set; }
    int ClockMin { get; set; }
    int ClockSec { get; set; }
    List<int>[] AvailableBadgePool { get; set; }
    List<int>[] BadgeShops { get; set; }
    List<int>[] BoardQuests { get; set; }
    List<int>[] Items { get; set; }
    List<int[]> Badges { get; set; }
    List<int[]> SamiraMusics { get; set; }
    List<int[]> StatBonus { get; set; }
    bool[,] LibraryStuff { get; set; }
    bool[] Flags { get; set; }
    string[] Flagstring { get; set; }
    int[] Flagvar { get; set; }
    bool[] RegionalFlags { get; set; }
    bool[] CrystalBFlags { get; set; }
    List<int> ExtraFollowers { get; set; }
    int[,] EnemyEncounter { get; set; }
}

internal sealed class PartyMemberRuntimeState
{
    public int Trueid { get; init; }
    public int Animid { get; init; }
    public int Hp { get; init; }
    public int Maxhp { get; init; }
    public int Basehp { get; init; }
    public int Atk { get; init; }
    public int Baseatk { get; init; }
    public int Def { get; init; }
    public int Basedef { get; init; }
}

internal sealed class GameDataRuntimeState : IGameDataRuntimeState
{
    public Vector3 PlayerPosition
    {
        get => MainManager.player.transform.position;
        set => MainManager.player.transform.position = value;
    }

    public int[] PartyOrder
    {
        get => MainManager.instance.partyorder;
        set => MainManager.instance.partyorder = value;
    }

    public List<PartyMemberRuntimeState> PlayerData
    {
        get => MainManager.instance.playerdata.Select(x => new PartyMemberRuntimeState
        {
            Trueid = x.trueid,
            Animid = x.animid,
            Hp = x.hp,
            Maxhp = x.maxhp,
            Basehp = x.basehp,
            Atk = x.atk,
            Baseatk = x.baseatk,
            Def = x.def,
            Basedef = x.basedef
        }).ToList();
        set => MainManager.instance.playerdata = value.Select(x => new MainManager.BattleData
        {
            trueid = x.Trueid,
            animid = x.Animid,
            hp = x.Hp,
            maxhp = x.Maxhp,
            basehp = x.Basehp,
            atk = x.Atk,
            baseatk = x.Baseatk,
            def = x.Def,
            basedef = x.Basedef,
            entityname = MainManager.menutext[46 + x.Trueid]
        }).ToArray();
    }

    public int MapAreaId { get => MainManager.instance.areaid; set => MainManager.instance.areaid = value; }
    public string MapName => MainManager.map.name;
    public int PartyLevel { get => MainManager.instance.partylevel; set => MainManager.instance.partylevel = value; }
    public int PartyExp { get => MainManager.instance.partyexp; set => MainManager.instance.partyexp = value; }
    public int NeededExp { get => MainManager.instance.neededexp; set => MainManager.instance.neededexp = value; }
    public int BaseTp { get => MainManager.instance.basetp; set => MainManager.instance.basetp = value; }
    public int Tp { get => MainManager.instance.tp; set => MainManager.instance.tp = value; }
    public int Money { get => MainManager.instance.money; set => MainManager.instance.money = value; }
    public int Bp { get => MainManager.instance.bp; set => MainManager.instance.bp = value; }
    public int MaxBp { get => MainManager.instance.maxbp; set => MainManager.instance.maxbp = value; }
    public int MaxItems { get => MainManager.instance.maxitems; set => MainManager.instance.maxitems = value; }
    public int MaxStorage { get => MainManager.instance.maxstorage; set => MainManager.instance.maxstorage = value; }
    public int ClockHour { get => MainManager.instance.clockhour; set => MainManager.instance.clockhour = value; }
    public int ClockMin { get => MainManager.instance.clockmin; set => MainManager.instance.clockmin = value; }
    public int ClockSec { get => MainManager.instance.clocksec; set => MainManager.instance.clocksec = value; }

    public List<int>[] AvailableBadgePool
    {
        get => MainManager.instance.avaliablebadgepool;
        set => MainManager.instance.avaliablebadgepool = value;
    }

    public List<int>[] BadgeShops
    {
        get => MainManager.instance.badgeshops;
        set => MainManager.instance.badgeshops = value;
    }

    public List<int>[] BoardQuests
    {
        get => MainManager.instance.boardquests;
        set => MainManager.instance.boardquests = value;
    }

    public List<int>[] Items { get => MainManager.instance.items; set => MainManager.instance.items = value; }
    public List<int[]> Badges { get => MainManager.instance.badges; set => MainManager.instance.badges = value; }

    public List<int[]> SamiraMusics
    {
        get => MainManager.instance.samiramusics;
        set => MainManager.instance.samiramusics = value;
    }

    public List<int[]> StatBonus
    {
        get => MainManager.instance.statbonus;
        set => MainManager.instance.statbonus = value;
    }

    public bool[,] LibraryStuff
    {
        get => MainManager.instance.librarystuff;
        set => MainManager.instance.librarystuff = value;
    }

    public bool[] Flags { get => MainManager.instance.flags; set => MainManager.instance.flags = value; }

    public string[] Flagstring
    {
        get => MainManager.instance.flagstring;
        set => MainManager.instance.flagstring = value;
    }

    public int[] Flagvar { get => MainManager.instance.flagvar; set => MainManager.instance.flagvar = value; }

    public bool[] RegionalFlags
    {
        get => MainManager.instance.regionalflags;
        set => MainManager.instance.regionalflags = value;
    }

    public bool[] CrystalBFlags
    {
        get => MainManager.instance.crystalbflags;
        set => MainManager.instance.crystalbflags = value;
    }

    public List<int> ExtraFollowers
    {
        get => MainManager.instance.extrafollowers;
        set => MainManager.instance.extrafollowers = value;
    }

    public int[,] EnemyEncounter
    {
        get => MainManager.instance.enemyencounter;
        set => MainManager.instance.enemyencounter = value;
    }
}