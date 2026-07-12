using UnityEngine;

namespace VenusRootLoader.Persistence;

internal interface ISaveDataPersistence
{
    bool SaveSlotExistsInVenusRootLoader(int saveSlot);
    MainManager.LoadData? LoadLiteSaveDataFromSlot(int saveSlot);
    MainManager.LoadData? LoadFullSaveDataFromSlot(int saveSlot);
    bool WriteSaveDataToCurrentSaveSlot(Vector3? playerPositionToSave);
}