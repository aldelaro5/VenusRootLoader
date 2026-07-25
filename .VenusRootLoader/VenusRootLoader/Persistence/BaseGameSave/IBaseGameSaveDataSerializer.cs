using UnityEngine;

namespace VenusRootLoader.Persistence.BaseGameSave;

internal interface IBaseGameSaveDataSerializer
{
    string GetBaseGameSaveDataFromRuntimeState(Vector3? playerPositionToSave);
}