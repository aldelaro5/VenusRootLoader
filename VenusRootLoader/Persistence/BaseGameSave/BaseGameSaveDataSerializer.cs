using CommunityToolkit.Diagnostics;
using System.Text;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Extensions;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Persistence.BaseGameSave;

internal sealed class BaseGameSaveDataSerializer : IBaseGameSaveDataSerializer
{
    private const char LineFeed = '\n';
    private const char Comma = ',';
    private const char AtSymbol = '@';
    private const string FlagstringSeparator = "|SPLIT|";
    private const char Dash = '-';

    private readonly IGameDataRuntimeState _gameDataRuntimeState;
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

    public BaseGameSaveDataSerializer(
        IGameDataRuntimeState gameDataRuntimeState,
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
        _gameDataRuntimeState = gameDataRuntimeState;
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

    public string GetBaseGameSaveDataFromRuntimeState(Vector3? playerPositionToSave)
    {
        StringBuilder sb = new(30_000);

        AppendHeaderLineStringData(sb, playerPositionToSave);
        AppendPlayerPartyMemberStatsLineStringData(sb);
        AppendGeneralInformationLineStringData(sb);
        AppendMedalShopsLineStringData(sb, _gameDataRuntimeState.AvailableBadgePool);
        AppendMedalShopsLineStringData(sb, _gameDataRuntimeState.BadgeShops);
        AppendQuestsLineStringData(sb);
        AppendItemsLineStringData(sb);
        AppendMedalsLineStringData(sb);
        AppendSamiraSongsLineStringData(sb);
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

    private void AppendHeaderLineStringData(StringBuilder sb, Vector3? playerPositionToSave)
    {
        Vector3 savePosition = playerPositionToSave ?? _gameDataRuntimeState.PlayerPosition;

        sb.AppendInvariant(savePosition.x).Append(Comma);
        sb.AppendInvariant(savePosition.y).Append(Comma);
        sb.AppendInvariant(savePosition.z).Append(Comma);
        sb.AppendInvariant(_gameDataRuntimeState.Flags[613]).Append(Comma);
        sb.AppendInvariant(_gameDataRuntimeState.Flags[614]).Append(Comma);
        sb.AppendInvariant(_gameDataRuntimeState.Flags[615]).Append(Comma);
        sb.AppendInvariant(_gameDataRuntimeState.Flags[616]).Append(Comma);
        sb.AppendInvariant(_gameDataRuntimeState.Flags[656]).Append(Comma);
        sb.AppendInvariant(_gameDataRuntimeState.Flags[681]).Append(Comma);
        sb.AppendInvariant(_gameDataRuntimeState.Flagstring[10]);

        sb.Append(LineFeed);
    }

    private void AppendPlayerPartyMemberStatsLineStringData(StringBuilder sb)
    {
        for (int i = 0; i < _gameDataRuntimeState.PlayerData.Count; i++)
        {
            if (i > 0)
                sb.Append(AtSymbol);

            PartyMemberRuntimeState battleData = _gameDataRuntimeState.PlayerData[i];
            string animIdEffectiveId = _animIdsLeafRegistry.LeavesByGameIds[battleData.Trueid].EffectiveId;

            sb.AppendInvariant(animIdEffectiveId).Append(Comma);
            sb.AppendInvariant(battleData.Hp).Append(Comma);
            sb.AppendInvariant(battleData.Maxhp).Append(Comma);
            sb.AppendInvariant(battleData.Basehp).Append(Comma);
            sb.AppendInvariant(battleData.Atk).Append(Comma);
            sb.AppendInvariant(battleData.Baseatk).Append(Comma);
            sb.AppendInvariant(battleData.Def).Append(Comma);
            sb.AppendInvariant(battleData.Basedef);
        }

        sb.Append(LineFeed);
    }

    private void AppendGeneralInformationLineStringData(StringBuilder sb)
    {
        string areaEffectiveId = _areasLeafRegistry.LeavesByGameIds[_gameDataRuntimeState.MapAreaId].EffectiveId;
        string mapEffectiveId = _mapsLeafRegistry.LeavesByGameIds[int.Parse(_gameDataRuntimeState.MapName)].EffectiveId;
        List<bool> progressIconFlags =
        [
            _gameDataRuntimeState.Flags[41],
            _gameDataRuntimeState.Flags[88],
            _gameDataRuntimeState.Flags[299],
            _gameDataRuntimeState.Flags[345],
            _gameDataRuntimeState.Flags[347],
            _gameDataRuntimeState.Flags[346],
            _gameDataRuntimeState.Flags[555]
        ];

        sb.AppendInvariant(_gameDataRuntimeState.PartyLevel).Append(Comma);
        sb.AppendInvariant(_gameDataRuntimeState.PartyExp).Append(Comma);
        sb.AppendInvariant(_gameDataRuntimeState.NeededExp).Append(Comma);
        sb.AppendInvariant(_gameDataRuntimeState.BaseTp).Append(Comma);
        sb.AppendInvariant(_gameDataRuntimeState.Tp).Append(Comma);
        sb.AppendInvariant(_gameDataRuntimeState.Money).Append(Comma);
        sb.AppendInvariant(mapEffectiveId).Append(Comma);
        sb.AppendInvariant(areaEffectiveId).Append(Comma);
        sb.AppendInvariant(_gameDataRuntimeState.Bp).Append(Comma);
        sb.AppendInvariant(_gameDataRuntimeState.MaxBp).Append(Comma);
        sb.AppendInvariant(_gameDataRuntimeState.MaxItems).Append(Comma);
        sb.AppendInvariant(_gameDataRuntimeState.MaxStorage).Append(Comma);
        sb.AppendInvariant(_gameDataRuntimeState.ClockHour).Append(Comma);
        sb.AppendInvariant(_gameDataRuntimeState.ClockMin).Append(Comma);
        sb.AppendInvariant(_gameDataRuntimeState.ClockSec).Append(Comma);
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
            sb.AppendInvariant(_medalsLeafRegistry.LeavesByGameIds[merabShopData[i]].EffectiveId);
        }

        sb.Append(AtSymbol);

        List<int> shadesShopData = shopsData[1];
        for (int i = 0; i < shadesShopData.Count; i++)
        {
            if (i > 0)
                sb.Append(Comma);
            sb.AppendInvariant(_medalsLeafRegistry.LeavesByGameIds[shadesShopData[i]].EffectiveId);
        }

        sb.Append(LineFeed);
    }

    private void AppendQuestsLineStringData(StringBuilder sb)
    {
        for (int i = 0; i < _gameDataRuntimeState.BoardQuests.Length; i++)
        {
            if (i > 0)
                sb.Append(AtSymbol);

            for (int j = 0; j < _gameDataRuntimeState.BoardQuests[i].Count; j++)
            {
                if (j > 0)
                    sb.Append(Comma);
                int questGameId = _gameDataRuntimeState.BoardQuests[i][j];
                sb.AppendInvariant(_questsLeafRegistry.LeavesByGameIds[questGameId].EffectiveId);
            }
        }

        sb.Append(LineFeed);
    }

    private void AppendItemsLineStringData(StringBuilder sb)
    {
        for (int i = 0; i < _gameDataRuntimeState.Items.Length; i++)
        {
            if (i > 0)
                sb.Append(AtSymbol);

            for (int j = 0; j < _gameDataRuntimeState.Items[i].Count; j++)
            {
                if (j > 0)
                    sb.Append(Comma);
                int itemGameId = _gameDataRuntimeState.Items[i][j];
                sb.AppendInvariant(_itemsLeafRegistry.LeavesByGameIds[itemGameId].EffectiveId);
            }
        }

        sb.Append(LineFeed);
    }

    private void AppendMedalsLineStringData(StringBuilder sb)
    {
        for (int i = 0; i < _gameDataRuntimeState.Badges.Count; i++)
        {
            if (i > 0)
                sb.Append(AtSymbol);

            int medalGameId = _gameDataRuntimeState.Badges[i][0];
            sb.AppendInvariant(_medalsLeafRegistry.LeavesByGameIds[medalGameId].EffectiveId);
            sb.Append(Comma);
            int medalEquipTarget = _gameDataRuntimeState.Badges[i][1];
            if (medalEquipTarget != -2)
                sb.AppendInvariant(_animIdsLeafRegistry.LeavesByGameIds[medalEquipTarget].EffectiveId);
        }

        sb.Append(LineFeed);
    }

    private void AppendSamiraSongsLineStringData(StringBuilder sb)
    {
        for (int i = 0; i < _gameDataRuntimeState.SamiraMusics.Count; i++)
        {
            if (i > 0)
                sb.Append(AtSymbol);

            int musicGameId = _gameDataRuntimeState.SamiraMusics[i][0];
            sb.AppendInvariant(_musicsLeafRegistry.LeavesByGameIds[musicGameId].EffectiveId);
            sb.Append(Comma);
            sb.AppendInvariant(_gameDataRuntimeState.SamiraMusics[i][1]);
        }

        sb.Append(LineFeed);
    }

    private void AppendStatBonusesLineStringData(StringBuilder sb)
    {
        for (int i = 0; i < _gameDataRuntimeState.StatBonus.Count; i++)
        {
            if (i > 0)
                sb.Append(AtSymbol);

            sb.AppendInvariant(_gameDataRuntimeState.StatBonus[i][0]);
            sb.Append(Comma);
            sb.AppendInvariant(_gameDataRuntimeState.StatBonus[i][1]);
            sb.Append(Comma);
            int bonusTarget = _gameDataRuntimeState.StatBonus[i][2];
            sb.AppendInvariant(_animIdsLeafRegistry.LeavesByGameIds[bonusTarget].EffectiveId);
        }

        sb.Append(LineFeed);
    }

    private void AppendLibraryLineStringData(StringBuilder sb)
    {
        for (int i = 0; i < _gameDataRuntimeState.LibraryStuff.GetLength(0); i++)
        {
            if (i > 0)
                sb.Append(AtSymbol);

            IEnumerable<Leaf> libraryPageLeaves = (MainManager.LibraryPages)i switch
            {
                MainManager.LibraryPages.Discoveries => _discoveriesLeafRegistry.LeavesByGameIds.Values,
                MainManager.LibraryPages.Bestiary => _enemiesLeafRegistry.LeavesByGameIds.Values,
                MainManager.LibraryPages.Recipes => _recipeLibraryEntriesLeafRegistry.LeavesByGameIds.Values,
                MainManager.LibraryPages.Logbook => _recordsLeafRegistry.LeavesByGameIds.Values,
                MainManager.LibraryPages.Map => _areasLeafRegistry.LeavesByGameIds.Values,
                _ => ThrowHelper.ThrowArgumentOutOfRangeException<IEnumerable<Leaf>>(
                    nameof(MainManager.LibraryPages))
            };

            int baseGameLeavesAmount = libraryPageLeaves.Count(l => l.CreatorId == Constants.BaseGameCreatorId);
            for (int j = 0; j < 256; j++)
            {
                if (j > 0)
                    sb.Append(Comma);
                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                if (j >= baseGameLeavesAmount)
                    sb.AppendInvariant(false);
                else
                    sb.AppendInvariant(_gameDataRuntimeState.LibraryStuff[i, j]);
            }
        }

        sb.Append(LineFeed);
    }

    private void AppendFlagsLineStringData(StringBuilder sb)
    {
        int baseGameAmount = _flagsLeafRegistry.LeavesByGameIds.Values
            .Count(f => f.CreatorId == Constants.BaseGameCreatorId);
        for (int i = 0; i < baseGameAmount; i++)
        {
            if (i > 0)
                sb.Append(Comma);
            sb.AppendInvariant(_gameDataRuntimeState.Flags[i]);
        }

        sb.Append(LineFeed);
    }

    private void AppendFlagstringsLineStringData(StringBuilder sb)
    {
        int baseGameAmount = _flagstringsLeafRegistry.LeavesByGameIds.Values
            .Count(f => f.CreatorId == Constants.BaseGameCreatorId);
        for (int i = 0; i < baseGameAmount; i++)
        {
            if (i > 0)
                sb.Append(FlagstringSeparator);

            string value = i switch
            {
                8 => !string.IsNullOrWhiteSpace(_gameDataRuntimeState.Flagstring[i])
                    ? ConvertChapter4CaptureData(_gameDataRuntimeState.Flagstring[i])
                    : _gameDataRuntimeState.Flagstring[i],
                12 => !string.IsNullOrWhiteSpace(_gameDataRuntimeState.Flagstring[i])
                    ? ConvertSavedSpyCardsDeck(_gameDataRuntimeState.Flagstring[i])
                    : _gameDataRuntimeState.Flagstring[i],
                13 => !string.IsNullOrWhiteSpace(_gameDataRuntimeState.Flagstring[i])
                    ? ConvertMysteryMedalsQueue(_gameDataRuntimeState.Flagstring[i])
                    : _gameDataRuntimeState.Flagstring[i],
                _ => _gameDataRuntimeState.Flagstring[i]
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
            string medalEffectiveId = _medalsLeafRegistry.LeavesByGameIds[medalGameId].EffectiveId;
            sb.AppendInvariant(medalEffectiveId);
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
            string spyCardEffectiveId = _spyCardsLeafRegistry.LeavesByGameIds[spyCardGameId].EffectiveId;
            sb.AppendInvariant(spyCardEffectiveId);
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
            string itemEffectiveId = _itemsLeafRegistry.LeavesByGameIds[itemGameId].EffectiveId;
            sb.AppendInvariant(itemEffectiveId);
        }

        sb.Append(Dash);

        for (int i = 0; i < keyItemGameIds.Length; i++)
        {
            if (i > 0)
                sb.Append(Comma);

            int itemGameId = int.Parse(keyItemGameIds[i]);
            string itemEffectiveId = _itemsLeafRegistry.LeavesByGameIds[itemGameId].EffectiveId;
            sb.AppendInvariant(itemEffectiveId);
        }

        sb.Append(Dash);
        sb.AppendInvariant(berryCount);

        return sb.ToString();
    }

    private void AppendFlagvarsLineStringData(StringBuilder sb)
    {
        int baseGameAmount = _flagvarsLeafRegistry.LeavesByGameIds.Values
            .Count(f => f.CreatorId == Constants.BaseGameCreatorId);
        for (int i = 0; i < baseGameAmount; i++)
        {
            if (i > 0)
                sb.Append(Comma);

            if (i == 56)
            {
                int itemGameId = _gameDataRuntimeState.Flagvar[i];
                if (itemGameId == 0)
                    continue;

                string itemEffectiveId = _itemsLeafRegistry.LeavesByGameIds[itemGameId].EffectiveId;
                sb.AppendInvariant(itemEffectiveId);
            }
            else
            {
                sb.AppendInvariant(_gameDataRuntimeState.Flagvar[i]);
            }
        }

        sb.Append(LineFeed);
    }

    private void AppendRegionalFlagsLineStringData(StringBuilder sb)
    {
        int baseGameAmount = 100;
        for (int i = 0; i < baseGameAmount; i++)
        {
            if (i > 0)
                sb.Append(Comma);
            sb.AppendInvariant(_gameDataRuntimeState.RegionalFlags[i]);
        }

        sb.Append(LineFeed);
    }

    private void AppendCryatalBerriesLineStringData(StringBuilder sb)
    {
        int baseGameAmount = _crystalBerriesLeafRegistry.LeavesByGameIds.Values
            .Count(f => f.CreatorId == Constants.BaseGameCreatorId);
        for (int i = 0; i < baseGameAmount; i++)
        {
            if (i > 0)
                sb.Append(Comma);
            sb.AppendInvariant(_gameDataRuntimeState.CrystalBFlags[i]);
        }

        sb.Append(LineFeed);
    }

    private void AppendFollowersLineStringData(StringBuilder sb)
    {
        for (int i = 0; i < _gameDataRuntimeState.ExtraFollowers.Count; i++)
        {
            if (i > 0)
                sb.Append(Comma);
            int followerAnimId = _gameDataRuntimeState.ExtraFollowers[i];
            string animIdEffectiveId = _animIdsLeafRegistry.LeavesByGameIds[followerAnimId].EffectiveId;
            sb.AppendInvariant(animIdEffectiveId);
        }

        sb.Append(LineFeed);
    }

    private void AppendEnemyEncountersDataLineStringData(StringBuilder sb)
    {
        int baseGameAmount = _enemiesLeafRegistry.LeavesByGameIds.Values
            .Count(f => f.CreatorId == Constants.BaseGameCreatorId);
        for (int i = 0; i < 256; i++)
        {
            if (i > 0)
                sb.Append(AtSymbol);
            if (i >= baseGameAmount)
            {
                sb.Append("0,0");
                continue;
            }

            sb.AppendInvariant(_gameDataRuntimeState.EnemyEncounter[i, 0]);
            sb.Append(Comma);
            sb.AppendInvariant(_gameDataRuntimeState.EnemyEncounter[i, 1]);
        }
    }
}