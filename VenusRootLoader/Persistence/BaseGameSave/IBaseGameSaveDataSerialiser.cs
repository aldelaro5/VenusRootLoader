using UnityEngine;

namespace VenusRootLoader.Persistence.BaseGameSave;

internal interface IBaseGameSaveDataSerialiser
{
    string GetBaseGameSaveDataFromRuntimeState(Vector3? playerPositionToSave);
}