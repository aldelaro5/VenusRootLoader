using UnityEngine;

namespace VenusRootLoader.Persistence;

internal interface ISaveDataPersistence
{
    bool WriteSaveDataToCurrentSaveSlot(Vector3? playerPositionToSave);
}