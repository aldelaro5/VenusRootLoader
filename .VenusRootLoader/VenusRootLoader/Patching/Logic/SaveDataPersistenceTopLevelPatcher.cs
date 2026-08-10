using HarmonyLib;
using UnityEngine;
using VenusRootLoader.Persistence;

namespace VenusRootLoader.Patching.Logic;

/// <summary>
/// This patcher implements VenusRootLoader's persistence system which allows data from the registry to be saved and loaded
/// regardless if the leaves were custom or from the base game.
/// <p>
/// It patches the following:
/// <list type="bullet">
/// <item><see cref="MainManager.Load"/>: If VenusRootLoader has a save for the given slot, we load that one into the game,
/// otherwise, we let the game load it as normal.</item>
/// <item><see cref="MainManager.Save"/>: Completely replaces the method to use our persistence system instead.</item>
/// </list>
/// </p>
/// </summary>
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