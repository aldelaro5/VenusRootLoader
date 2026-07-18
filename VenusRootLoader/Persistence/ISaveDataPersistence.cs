using UnityEngine;

namespace VenusRootLoader.Persistence;

// TODO: Implement copy and delete
internal interface ISaveDataPersistence
{
    bool SaveSlotExistsInVenusRootLoader(int saveSlot);
    MainManager.LoadData? LoadLiteSaveDataFromSlot(int saveSlot);
    MainManager.LoadData? LoadFullSaveDataFromSlot(int saveSlot);
    bool WriteSaveDataToSaveSlot(int saveSlot, Vector3? playerPositionToSave);
}