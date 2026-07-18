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
    internal static bool LoadSaveData(int file, bool lite, ref MainManager.LoadData? __result)
    {
        if (!_instance._saveDataPersistence.SaveSlotExistsInVenusRootLoader(file))
            return true;

        if (lite)
        {
            __result = _instance._saveDataPersistence.LoadLiteSaveDataFromSlot(file);
            return false;
        }

        __result = _instance._saveDataPersistence.LoadFullSaveDataFromSlot(file);
        // This is necessary for the stats calc and HUD to work properly since the save loading effectively did a ChangeParty
        MainManager.RebuildHUD();
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(MainManager), nameof(MainManager.Save))]
    // ReSharper disable once InconsistentNaming
    internal static bool WriteSaveData(Vector3? savepos, ref bool __result)
    {
        __result = _instance._saveDataPersistence.WriteSaveDataToSaveSlot(MainManager.saveslot, savepos);
        return false;
    }
}