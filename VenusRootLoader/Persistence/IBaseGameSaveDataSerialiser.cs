using UnityEngine;

namespace VenusRootLoader.Persistence;

internal interface IBaseGameSaveDataSerialiser
{
    string GetBaseGameSaveDataFromRuntimeState(Vector3? playerPositionToSave);
}