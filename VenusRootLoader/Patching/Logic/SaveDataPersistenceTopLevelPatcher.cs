using HarmonyLib;
using UnityEngine;
using VenusRootLoader.Persistence;

namespace VenusRootLoader.Patching.Logic;

internal sealed class SaveDataPersistenceTopLevelPatcher : ITopLevelPatcher
{
    private readonly IHarmonyTypePatcher _harmonyTypePatcher;
    private readonly ISaveDataPersistence _saveDataPersistence;

    private static SaveDataPersistenceTopLevelPatcher _instance = null!;

    public SaveDataPersistenceTopLevelPatcher(
        IHarmonyTypePatcher harmonyTypePatcher,
        ISaveDataPersistence saveDataPersistence)
    {
        _instance = this;
        _harmonyTypePatcher = harmonyTypePatcher;
        _saveDataPersistence = saveDataPersistence;
    }

    public void Patch() => _harmonyTypePatcher.PatchAll(typeof(SaveDataPersistenceTopLevelPatcher));

    [HarmonyPrefix]
    [HarmonyPatch(typeof(MainManager), nameof(MainManager.Load))]
    // ReSharper disable once InconsistentNaming
    internal static bool WriteSaveData(int file, bool lite, ref MainManager.LoadData? __result)
    {
        if (!lite)
            return true;

        if (!_instance._saveDataPersistence.SaveSlotExistsInVenusRootLoader(file))
            return true;

        __result = _instance._saveDataPersistence.LoadLiteSaveDataFromSlot(file);
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(MainManager), nameof(MainManager.Save))]
    // ReSharper disable once InconsistentNaming
    internal static bool WriteSaveData(Vector3? savepos, ref bool __result)
    {
        __result = _instance._saveDataPersistence.WriteSaveDataToCurrentSaveSlot(savepos);
        return false;
    }
}