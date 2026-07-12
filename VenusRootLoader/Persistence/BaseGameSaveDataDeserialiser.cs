using CommunityToolkit.Diagnostics;
using System.Globalization;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Persistence;

internal sealed class BaseGameSaveDataDeserialiser : IBaseGameSaveDataDeserialiser
{
    private readonly ILeavesRegistry<MapLeaf> _mapsLeafRegistry;
    private readonly ILeavesRegistry<AreaLeaf> _areasLeafRegistry;

    public BaseGameSaveDataDeserialiser(
        ILeavesRegistry<MapLeaf> mapsLeafRegistry,
        ILeavesRegistry<AreaLeaf> areasLeafRegistry)
    {
        _mapsLeafRegistry = mapsLeafRegistry;
        _areasLeafRegistry = areasLeafRegistry;
    }

    public MainManager.LoadData DeserialiseLiteBaseGameSaveData(string saveData)
    {
        string[] baseGameSaveDataLines = saveData.Split(StringUtils.NewlineSplitDelimiter);
        if (baseGameSaveDataLines.Length < 3)
            ThrowHelper.ThrowInvalidDataException("There are less than 3 lines in the base game save data");

        string[] baseGameHeaderData = baseGameSaveDataLines[0].Split(StringUtils.CommaSplitDelimiter);
        if (baseGameHeaderData.Length < 10)
        {
            ThrowHelper.ThrowInvalidDataException(
                "There are less than 10 fields in the base game save data header line");
        }

        string[] baseGameGeneralInformationData = baseGameSaveDataLines[2].Split(StringUtils.CommaSplitDelimiter);
        if (baseGameGeneralInformationData.Length < 16)
        {
            ThrowHelper.ThrowInvalidDataException(
                "There are less than 16 fields in the base game save data general information line");
        }

        string mapNamedId = baseGameGeneralInformationData[6];
        if (!_mapsLeafRegistry.LeavesByNamedIds.TryGetValue(mapNamedId, out MapLeaf mapLeaf))
        {
            ThrowHelper.ThrowInvalidDataException(
                $"The save was done at a {nameof(MapLeaf)} named {mapNamedId} while no such {nameof(MapLeaf)} exists in the registry.");
        }

        string areaNamedId = baseGameGeneralInformationData[7];
        if (!_areasLeafRegistry.LeavesByNamedIds.TryGetValue(areaNamedId, out AreaLeaf areaLeaf))
        {
            ThrowHelper.ThrowInvalidDataException(
                $"The save was done at an {nameof(AreaLeaf)} named {areaNamedId} while no such {nameof(AreaLeaf)} exists in the registry.");
        }

        MainManager.LoadData loadData = new();
        loadData.challenges =
        [
            bool.Parse(baseGameHeaderData[3]),
            bool.Parse(baseGameHeaderData[4]),
            bool.Parse(baseGameHeaderData[5]),
            bool.Parse(baseGameHeaderData[6]),
            bool.Parse(baseGameHeaderData[7]),
            bool.Parse(baseGameHeaderData[8]),
        ];
        loadData.filename = baseGameHeaderData[9];

        loadData.level = int.Parse(baseGameGeneralInformationData[0], CultureInfo.InvariantCulture);
        loadData.mapid = mapLeaf.GameId;
        loadData.areaid = areaLeaf.GameId;
        loadData.timeh = int.Parse(baseGameGeneralInformationData[12], CultureInfo.InvariantCulture);
        loadData.timem = int.Parse(baseGameGeneralInformationData[13], CultureInfo.InvariantCulture);
        loadData.times = int.Parse(baseGameGeneralInformationData[14], CultureInfo.InvariantCulture);
        loadData.progression = int.Parse(baseGameGeneralInformationData[15], CultureInfo.InvariantCulture);

        return loadData;
    }

    public MainManager.LoadData DeserialiseFullBaseGameSaveData(string saveData)
    {
        throw new NotImplementedException();
    }
}