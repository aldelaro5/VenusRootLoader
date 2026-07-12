using CommunityToolkit.Diagnostics;
using System.Text;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Extensions;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Persistence;

internal sealed class BaseGameSaveDataSerialiser : IBaseGameSaveDataSerialiser
{
    private const char LineFeed = '\n';
    private const char Comma = ',';
    private const char AtSymbol = '@';
    private const string FlagstringSeparator = "|SPLIT|";
    private const char Dash = '-';

    private readonly ILeavesRegistry<AnimIdLeaf> _animIdsLeafRegistry;
    private readonly ILeavesRegistry<AreaLeaf> _areasLeafRegistry;
    private readonly ILeavesRegistry<MapLeaf> _mapsLeafRegistry;
    private readonly ILeavesRegistry<MedalLeaf> _medalsLeafRegistry;
    private readonly ILeavesRegistry<QuestLeaf> _questsLeafRegistry;
    private readonly ILeavesRegistry<ItemLeaf> _itemsLeafRegistry;
    private readonly ILeavesRegistry<MusicLeaf> _musicsLeafRegistry;
    private readonly ILeavesRegistry<DiscoveryLeaf> _discoveriesLeafRegistry;
    private readonly ILeavesRegistry<EnemyLeaf> _enemiesLeafRegistry;
    private readonly ILeavesRegistry<RecipeLibraryEntryLeaf> _recipeLibraryEntriesLeafRegistry;
    private readonly ILeavesRegistry<RecordLeaf> _recordsLeafRegistry;
    private readonly ILeavesRegistry<FlagLeaf> _flagsLeafRegistry;
    private readonly ILeavesRegistry<FlagstringLeaf> _flagstringsLeafRegistry;
    private readonly ILeavesRegistry<SpyCardLeaf> _spyCardsLeafRegistry;
    private readonly ILeavesRegistry<FlagvarLeaf> _flagvarsLeafRegistry;
    private readonly ILeavesRegistry<CrystalBerryLeaf> _crystalBerriesLeafRegistry;

    public BaseGameSaveDataSerialiser(
        ILeavesRegistry<AnimIdLeaf> animIdsLeafRegistry,
        ILeavesRegistry<MapLeaf> mapsLeafRegistry,
        ILeavesRegistry<AreaLeaf> areasLeafRegistry,
        ILeavesRegistry<MedalLeaf> medalsLeafRegistry,
        ILeavesRegistry<QuestLeaf> questsLeafRegistry,
        ILeavesRegistry<ItemLeaf> itemsLeafRegistry,
        ILeavesRegistry<MusicLeaf> musicsLeafRegistry,
        ILeavesRegistry<DiscoveryLeaf> discoveriesLeafRegistry,
        ILeavesRegistry<EnemyLeaf> enemiesLeafRegistry,
        ILeavesRegistry<RecipeLibraryEntryLeaf> recipeLibraryEntriesLeafRegistry,
        ILeavesRegistry<RecordLeaf> recordsLeafRegistry,
        ILeavesRegistry<FlagLeaf> flagsLeafRegistry,
        ILeavesRegistry<FlagstringLeaf> flagstringsLeafRegistry,
        ILeavesRegistry<SpyCardLeaf> spyCardsLeafRegistry,
        ILeavesRegistry<FlagvarLeaf> flagvarsLeafRegistry,
        ILeavesRegistry<CrystalBerryLeaf> crystalBerriesLeafRegistry)
    {
        _animIdsLeafRegistry = animIdsLeafRegistry;
        _areasLeafRegistry = areasLeafRegistry;
        _mapsLeafRegistry = mapsLeafRegistry;
        _medalsLeafRegistry = medalsLeafRegistry;
        _questsLeafRegistry = questsLeafRegistry;
        _itemsLeafRegistry = itemsLeafRegistry;
        _musicsLeafRegistry = musicsLeafRegistry;
        _discoveriesLeafRegistry = discoveriesLeafRegistry;
        _enemiesLeafRegistry = enemiesLeafRegistry;
        _recipeLibraryEntriesLeafRegistry = recipeLibraryEntriesLeafRegistry;
        _recordsLeafRegistry = recordsLeafRegistry;
        _flagsLeafRegistry = flagsLeafRegistry;
        _flagstringsLeafRegistry = flagstringsLeafRegistry;
        _spyCardsLeafRegistry = spyCardsLeafRegistry;
        _flagvarsLeafRegistry = flagvarsLeafRegistry;
        _crystalBerriesLeafRegistry = crystalBerriesLeafRegistry;
    }

    public string GetSaveDataFromRuntimeState(Vector3? playerPositionToSave)
    {
        StringBuilder sb = new(30_000);
        MainManager mainManager = MainManager.instance;

        AppendHeaderLineStringData(sb, playerPositionToSave);
        AppendPlayerPartyMemberStatsLineStringData(sb);
        AppendGeneralInformationLineStringData(sb);
        AppendMedalShopsLineStringData(sb, mainManager.avaliablebadgepool);
        AppendMedalShopsLineStringData(sb, mainManager.badgeshops);
        AppendQuestsLineStringData(sb);
        AppendItemsLineStringData(sb);
        AppendMedalsLineStringData(sb);
        AppendMusicsLineStringData(sb);
        AppendStatBonusesLineStringData(sb);
        AppendLibraryLineStringData(sb);
        AppendFlagsLineStringData(sb);
        AppendFlagstringsLineStringData(sb);
        AppendFlagvarsLineStringData(sb);
        AppendRegionalFlagsLineStringData(sb);
        AppendCryatalBerriesLineStringData(sb);
        AppendFollowersLineStringData(sb);
        AppendEnemyEncountersDataLineStringData(sb);

        return sb.ToString();
    }

    private static void AppendHeaderLineStringData(StringBuilder sb, Vector3? playerPositionToSave)
    {
        Vector3 savePosition = playerPositionToSave ?? MainManager.player.transform.position;
        MainManager mainManager = MainManager.instance;

        sb.AppendInvariant(savePosition.x).Append(Comma);
        sb.AppendInvariant(savePosition.y).Append(Comma);
        sb.AppendInvariant(savePosition.z).Append(Comma);
        sb.AppendInvariant(mainManager.flags[613]).Append(Comma);
        sb.AppendInvariant(mainManager.flags[614]).Append(Comma);
        sb.AppendInvariant(mainManager.flags[615]).Append(Comma);
        sb.AppendInvariant(mainManager.flags[616]).Append(Comma);
        sb.AppendInvariant(mainManager.flags[656]).Append(Comma);
        sb.AppendInvariant(mainManager.flags[681]).Append(Comma);
        sb.AppendInvariant(mainManager.flagstring[10]);

        sb.Append(LineFeed);
    }

    private void AppendPlayerPartyMemberStatsLineStringData(StringBuilder sb)
    {
        for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
        {
            if (i > 0)
                sb.Append(AtSymbol);

            MainManager.BattleData battleData = MainManager.instance.playerdata[i];
            string animIdNamedId = _animIdsLeafRegistry.LeavesByGameIds[battleData.trueid].NamedId;

            sb.AppendInvariant(animIdNamedId).Append(Comma);
            sb.AppendInvariant(battleData.hp).Append(Comma);
            sb.AppendInvariant(battleData.maxhp).Append(Comma);
            sb.AppendInvariant(battleData.basehp).Append(Comma);
            sb.AppendInvariant(battleData.atk).Append(Comma);
            sb.AppendInvariant(battleData.baseatk).Append(Comma);
            sb.AppendInvariant(battleData.def).Append(Comma);
            sb.AppendInvariant(battleData.basedef);
        }

        sb.Append(LineFeed);
    }

    private void AppendGeneralInformationLineStringData(StringBuilder sb)
    {
        string areaNamedId = _areasLeafRegistry.LeavesByGameIds[(int)MainManager.map.areaid].NamedId;
        string mapNamedId = _mapsLeafRegistry.LeavesByGameIds[int.Parse(MainManager.map.name)].NamedId;
        MainManager mainManager = MainManager.instance;
        List<bool> progressIconFlags =
        [
            mainManager.flags[41],
            mainManager.flags[88],
            mainManager.flags[299],
            mainManager.flags[345],
            mainManager.flags[347],
            mainManager.flags[346],
            mainManager.flags[555],
        ];

        sb.AppendInvariant(mainManager.partylevel).Append(Comma);
        sb.AppendInvariant(mainManager.partyexp).Append(Comma);
        sb.AppendInvariant(mainManager.neededexp).Append(Comma);
        sb.AppendInvariant(mainManager.basetp).Append(Comma);
        sb.AppendInvariant(mainManager.tp).Append(Comma);
        sb.AppendInvariant(mainManager.money).Append(Comma);
        sb.AppendInvariant(mapNamedId).Append(Comma);
        sb.AppendInvariant(areaNamedId).Append(Comma);
        sb.AppendInvariant(mainManager.bp).Append(Comma);
        sb.AppendInvariant(mainManager.maxbp).Append(Comma);
        sb.AppendInvariant(mainManager.maxitems).Append(Comma);
        sb.AppendInvariant(mainManager.maxstorage).Append(Comma);
        sb.AppendInvariant(mainManager.clockhour).Append(Comma);
        sb.AppendInvariant(mainManager.clockmin).Append(Comma);
        sb.AppendInvariant(mainManager.clocksec).Append(Comma);
        sb.AppendInvariant(progressIconFlags.Count(flag => flag));

        sb.Append(LineFeed);
    }

    private void AppendMedalShopsLineStringData(StringBuilder sb, List<int>[] shopsData)
    {
        List<int> merabShopData = shopsData[0];
        for (int i = 0; i < merabShopData.Count; i++)
        {
            if (i > 0)
                sb.Append(Comma);
            sb.AppendInvariant(_medalsLeafRegistry.LeavesByGameIds[merabShopData[i]].NamedId);
        }

        sb.Append(AtSymbol);

        List<int> shadesShopData = shopsData[1];
        for (int i = 0; i < shadesShopData.Count; i++)
        {
            if (i > 0)
                sb.Append(Comma);
            sb.AppendInvariant(_medalsLeafRegistry.LeavesByGameIds[shopsData[0][i]].NamedId);
        }

        sb.Append(LineFeed);
    }

    private void AppendQuestsLineStringData(StringBuilder sb)
    {
        MainManager mainManager = MainManager.instance;

        for (int i = 0; i < mainManager.boardquests.Length; i++)
        {
            if (i > 0)
                sb.Append(AtSymbol);

            for (int j = 0; j < mainManager.boardquests[i].Count; j++)
            {
                if (j > 0)
                    sb.Append(Comma);
                int questGameId = mainManager.boardquests[i][j];
                sb.AppendInvariant(_questsLeafRegistry.LeavesByGameIds[questGameId].NamedId);
            }
        }

        sb.Append(LineFeed);
    }

    private void AppendItemsLineStringData(StringBuilder sb)
    {
        MainManager mainManager = MainManager.instance;

        for (int i = 0; i < mainManager.items.Length; i++)
        {
            if (i > 0)
                sb.Append(AtSymbol);

            for (int j = 0; j < mainManager.items[i].Count; j++)
            {
                if (j > 0)
                    sb.Append(Comma);
                int itemGameId = mainManager.items[i][j];
                sb.AppendInvariant(_itemsLeafRegistry.LeavesByGameIds[itemGameId].NamedId);
            }
        }

        sb.Append(LineFeed);
    }

    private void AppendMedalsLineStringData(StringBuilder sb)
    {
        MainManager mainManager = MainManager.instance;

        for (int i = 0; i < mainManager.badges.Count; i++)
        {
            if (i > 0)
                sb.Append(AtSymbol);

            int medalGameId = mainManager.badges[i][0];
            sb.AppendInvariant(_medalsLeafRegistry.LeavesByGameIds[medalGameId].NamedId);
            sb.Append(Comma);
            int medalEquipTarget = mainManager.badges[i][1];
            if (medalEquipTarget != -2)
                sb.AppendInvariant(_animIdsLeafRegistry.LeavesByGameIds[medalEquipTarget].NamedId);
        }

        sb.Append(LineFeed);
    }

    private void AppendMusicsLineStringData(StringBuilder sb)
    {
        MainManager mainManager = MainManager.instance;

        for (int i = 0; i < mainManager.samiramusics.Count; i++)
        {
            if (i > 0)
                sb.Append(AtSymbol);

            int musicGameId = mainManager.samiramusics[i][0];
            sb.AppendInvariant(_musicsLeafRegistry.LeavesByGameIds[musicGameId].NamedId);
            sb.Append(Comma);
            sb.AppendInvariant(mainManager.samiramusics[i][1]);
        }

        sb.Append(LineFeed);
    }

    private void AppendStatBonusesLineStringData(StringBuilder sb)
    {
        MainManager mainManager = MainManager.instance;

        for (int i = 0; i < mainManager.statbonus.Count; i++)
        {
            if (i > 0)
                sb.Append(AtSymbol);

            sb.AppendInvariant(mainManager.statbonus[i][0]);
            sb.Append(Comma);
            sb.AppendInvariant(mainManager.statbonus[i][1]);
            sb.Append(Comma);
            int bonusTarget = mainManager.statbonus[i][2];
            sb.AppendInvariant(_animIdsLeafRegistry.LeavesByGameIds[bonusTarget].NamedId);
        }

        sb.Append(LineFeed);
    }

    private void AppendLibraryLineStringData(StringBuilder sb)
    {
        MainManager mainManager = MainManager.instance;

        for (int i = 0; i < mainManager.librarystuff.GetLength(0); i++)
        {
            if (i > 0)
                sb.Append(AtSymbol);

            IEnumerable<Leaf> libraryPageLeaves = (MainManager.LibraryPages)i switch
            {
                MainManager.LibraryPages.Discoveries => _discoveriesLeafRegistry.LeavesByNamedIds.Values,
                MainManager.LibraryPages.Bestiary => _enemiesLeafRegistry.LeavesByNamedIds.Values,
                MainManager.LibraryPages.Recipes => _recipeLibraryEntriesLeafRegistry.LeavesByNamedIds.Values,
                MainManager.LibraryPages.Logbook => _recordsLeafRegistry.LeavesByNamedIds.Values,
                MainManager.LibraryPages.Map => _areasLeafRegistry.LeavesByNamedIds.Values,
                _ => ThrowHelper.ThrowArgumentOutOfRangeException<IEnumerable<Leaf>>(
                    nameof(MainManager.LibraryPages))
            };

            int baseGameLeavesAmount = libraryPageLeaves.Count(l => l.CreatorId == Constants.BaseGameId);
            for (int j = 0; j < 256; j++)
            {
                if (j > 0)
                    sb.Append(Comma);
                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                if (j >= baseGameLeavesAmount)
                    sb.AppendInvariant(false);
                else
                    sb.AppendInvariant(mainManager.librarystuff[i, j]);
            }
        }

        sb.Append(LineFeed);
    }

    private void AppendFlagsLineStringData(StringBuilder sb)
    {
        MainManager mainManager = MainManager.instance;

        int baseGameAmount = _flagsLeafRegistry.LeavesByNamedIds.Values
            .Count(f => f.CreatorId == Constants.BaseGameId);
        for (int i = 0; i < baseGameAmount; i++)
        {
            if (i > 0)
                sb.Append(Comma);
            sb.AppendInvariant(mainManager.flags[i]);
        }

        sb.Append(LineFeed);
    }

    private void AppendFlagstringsLineStringData(StringBuilder sb)
    {
        MainManager mainManager = MainManager.instance;

        int baseGameAmount = _flagstringsLeafRegistry.LeavesByNamedIds.Values
            .Count(f => f.CreatorId == Constants.BaseGameId);
        for (int i = 0; i < baseGameAmount; i++)
        {
            if (i > 0)
                sb.Append(FlagstringSeparator);

            string value = i switch
            {
                8 => !string.IsNullOrWhiteSpace(mainManager.flagstring[i])
                    ? ConvertChapter4CaptureData(mainManager.flagstring[i])
                    : mainManager.flagstring[i],
                12 => !string.IsNullOrWhiteSpace(mainManager.flagstring[i])
                    ? ConvertSavedSpyCardsDeck(mainManager.flagstring[i])
                    : mainManager.flagstring[i],
                13 => !string.IsNullOrWhiteSpace(mainManager.flagstring[i])
                    ? ConvertMysteryMedalsQueue(mainManager.flagstring[i])
                    : mainManager.flagstring[i],
                _ => mainManager.flagstring[i]
            };
            sb.AppendInvariant(value);
        }

        sb.Append(LineFeed);
    }

    private string ConvertMysteryMedalsQueue(string original)
    {
        StringBuilder sb = new();

        string[] originalGameIds = original.Split(StringUtils.CommaSplitDelimiter);
        for (int i = 0; i < originalGameIds.Length; i++)
        {
            if (i > 0)
                sb.Append(Comma);

            int medalGameId = int.Parse(originalGameIds[i]);
            string medalNamedId = _medalsLeafRegistry.LeavesByGameIds[medalGameId].NamedId;
            sb.AppendInvariant(medalNamedId);
        }

        return sb.ToString();
    }

    private string ConvertSavedSpyCardsDeck(string original)
    {
        StringBuilder sb = new();

        string[] originalGameIds = original.Split(StringUtils.CommaSplitDelimiter);
        for (int i = 0; i < originalGameIds.Length; i++)
        {
            if (i > 0)
                sb.Append(Comma);

            int spyCardGameId = int.Parse(originalGameIds[i]);
            string spyCardNamedId = _spyCardsLeafRegistry.LeavesByGameIds[spyCardGameId].NamedId;
            sb.AppendInvariant(spyCardNamedId);
        }

        return sb.ToString();
    }

    private string ConvertChapter4CaptureData(string original)
    {
        StringBuilder sb = new();

        string[] originalParts = original.Split(StringUtils.DashSplitDelimiter);
        string[] regularItemGameIds = originalParts[0].Split(StringUtils.CommaSplitDelimiter);
        string[] keyItemGameIds = originalParts[1].Split(StringUtils.CommaSplitDelimiter);
        string berryCount = originalParts[2];

        for (int i = 0; i < regularItemGameIds.Length; i++)
        {
            if (i > 0)
                sb.Append(Comma);

            int itemGameId = int.Parse(regularItemGameIds[i]);
            string itemNamedId = _itemsLeafRegistry.LeavesByGameIds[itemGameId].NamedId;
            sb.AppendInvariant(itemNamedId);
        }

        sb.Append(Dash);

        for (int i = 0; i < keyItemGameIds.Length; i++)
        {
            if (i > 0)
                sb.Append(Comma);

            int itemGameId = int.Parse(keyItemGameIds[i]);
            string itemNamedId = _itemsLeafRegistry.LeavesByGameIds[itemGameId].NamedId;
            sb.AppendInvariant(itemNamedId);
        }

        sb.Append(Dash);
        sb.AppendInvariant(berryCount);

        return sb.ToString();
    }

    private void AppendFlagvarsLineStringData(StringBuilder sb)
    {
        MainManager mainManager = MainManager.instance;

        int baseGameAmount = _flagvarsLeafRegistry.LeavesByNamedIds.Values
            .Count(f => f.CreatorId == Constants.BaseGameId);
        for (int i = 0; i < baseGameAmount; i++)
        {
            if (i > 0)
                sb.Append(Comma);

            if (i == 56)
            {
                int itemGameId = mainManager.flagvar[i];
                if (itemGameId == 0)
                    continue;

                string itemNamedId = _itemsLeafRegistry.LeavesByGameIds[itemGameId].NamedId;
                sb.AppendInvariant(itemNamedId);
            }
            else
            {
                sb.AppendInvariant(mainManager.flagvar[i]);
            }
        }

        sb.Append(LineFeed);
    }

    private static void AppendRegionalFlagsLineStringData(StringBuilder sb)
    {
        MainManager mainManager = MainManager.instance;

        int baseGameAmount = 100;
        for (int i = 0; i < baseGameAmount; i++)
        {
            if (i > 0)
                sb.Append(Comma);
            sb.AppendInvariant(mainManager.regionalflags[i]);
        }

        sb.Append(LineFeed);
    }

    private void AppendCryatalBerriesLineStringData(StringBuilder sb)
    {
        MainManager mainManager = MainManager.instance;

        int baseGameAmount = _crystalBerriesLeafRegistry.LeavesByNamedIds.Values
            .Count(f => f.CreatorId == Constants.BaseGameId);
        for (int i = 0; i < baseGameAmount; i++)
        {
            if (i > 0)
                sb.Append(Comma);
            sb.AppendInvariant(mainManager.crystalbflags[i]);
        }

        sb.Append(LineFeed);
    }

    private void AppendFollowersLineStringData(StringBuilder sb)
    {
        MainManager mainManager = MainManager.instance;

        for (int i = 0; i < mainManager.extrafollowers.Count; i++)
        {
            if (i > 0)
                sb.Append(Comma);
            int followerAnimId = mainManager.extrafollowers[i];
            string animIdNamedId = _animIdsLeafRegistry.LeavesByGameIds[followerAnimId].NamedId;
            sb.AppendInvariant(animIdNamedId);
        }

        sb.Append(LineFeed);
    }

    private void AppendEnemyEncountersDataLineStringData(StringBuilder sb)
    {
        MainManager mainManager = MainManager.instance;

        int baseGameAmount = _enemiesLeafRegistry.LeavesByNamedIds.Values
            .Count(f => f.CreatorId == Constants.BaseGameId);
        for (int i = 0; i < 256; i++)
        {
            if (i > 0)
                sb.Append(AtSymbol);
            if (i >= baseGameAmount)
            {
                sb.Append("0,0");
                continue;
            }

            sb.AppendInvariant(mainManager.enemyencounter[i, 0]);
            sb.Append(Comma);
            sb.AppendInvariant(mainManager.enemyencounter[i, 1]);
        }
    }
}