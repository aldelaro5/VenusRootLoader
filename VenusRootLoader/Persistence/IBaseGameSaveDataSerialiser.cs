using UnityEngine;

namespace VenusRootLoader.Persistence;

internal interface IBaseGameSaveDataSerialiser
{
    string GetSaveDataFromRuntimeState(Vector3? playerPositionToSave);
}