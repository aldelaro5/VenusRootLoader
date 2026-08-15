using UnityEngine;

namespace VenusRootLoader.Persistence;

internal interface ISaveDataPersistence
{
    bool SaveSlotExistsInVenusRootLoader(int saveSlot);
    MainManager.LoadData? LoadLiteSaveDataFromSlot(int saveSlot);
    MainManager.LoadData? LoadFullSaveDataFromSlot(int saveSlot);
    bool WriteSaveDataToSaveSlot(int saveSlot, Vector3? playerPositionToSave);
    bool CopySaveSlot(int sourceSaveSlot, int destinationSaveSlot);
    bool DeleteSaveSlot(int saveSlot);
}