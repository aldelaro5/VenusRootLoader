using InputIOManager;
using MonoDetour.Cil;
using MonoDetour.DetourTypes;
using MonoMod.Cil;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using UnityEngine;
using VenusRootLoader.Extensions;
using VenusRootLoader.Persistence;
using VenusRootLoader.Utility;
using OpCodes = Mono.Cecil.Cil.OpCodes;

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
/// <item><see cref="InputIO.DeleteFile"/>: If VenusRootLoader has a save for the given slot, we delete it, and we let
/// the game delete its as normal too.</item>
/// <item><see cref="StartMenu.DoCopy"/>: Patch the copy process such that if VenusRootLoader has a save for the given
/// source slot, we copy it, and we do the same as the base game if it exists too or if there was no VenusRootLoader save
/// to start with.</item>
/// </list>
/// </p>
/// </summary>
[SuppressMessage("System.IO.Abstractions", "IO0002:Replace File class with IFileSystem.File for improved testability")]
internal sealed class SaveDataPersistenceTopLevelPatcher : ITopLevelPatcher
{
    private static SaveDataPersistenceTopLevelPatcher _instance = null!;

    private static readonly bool IsGogEdition = typeof(MainManager).Assembly.DefinedTypes.Any(x =>
        x.Namespace is null && x.Name == "GalaxyManager" && x.BaseType == typeof(MonoBehaviour));

    private static readonly string BaseGameSavesPrefix = IsGogEdition ? "Saves/save" : "save";

    private readonly ISaveDataPersistence _saveDataPersistence;

    public SaveDataPersistenceTopLevelPatcher(ISaveDataPersistence saveDataPersistence)
    {
        _instance = this;
        _saveDataPersistence = saveDataPersistence;
    }

    public void Patch()
    {
        Md.MainManager.Load.ControlFlowPrefix(LoadSaveData);
        Md.MainManager.Save.ControlFlowPrefix(WriteSaveData);
        Md.InputIOManager.InputIO.DeleteFile.ControlFlowPrefix(DeleteSaveData);
        Md.StartMenu.DoCopy.ILHookMoveNext(PatchCopySaveData);
    }

    private static ReturnFlow LoadSaveData(ref int file, ref bool lite, ref MainManager.LoadData? result)
    {
        if (!_instance._saveDataPersistence.SaveSlotExistsInVenusRootLoader(file))
            return ReturnFlow.None;

        if (lite)
        {
            result = _instance._saveDataPersistence.LoadLiteSaveDataFromSlot(file);
            return ReturnFlow.SkipOriginal;
        }

        result = _instance._saveDataPersistence.LoadFullSaveDataFromSlot(file);
        // This is necessary for the stats calc and HUD to work properly since the save loading effectively did a ChangeParty
        MainManager.RebuildHUD();
        return ReturnFlow.SkipOriginal;
    }

    private static ReturnFlow WriteSaveData(ref Vector3? savepos, ref bool result)
    {
        result = _instance._saveDataPersistence.WriteSaveDataToSaveSlot(MainManager.saveslot, savepos);
        return ReturnFlow.SkipOriginal;
    }

    private static ReturnFlow DeleteSaveData(ref string path, ref bool result)
    {
        int saveSlot = int.Parse(path.Replace(BaseGameSavesPrefix, "").Replace(".dat", ""));
        if (!_instance._saveDataPersistence.SaveSlotExistsInVenusRootLoader(saveSlot))
            return ReturnFlow.None;

        result = _instance._saveDataPersistence.DeleteSaveSlot(saveSlot);
        return result && File.Exists(BaseGameSavesPrefix + MainManager.instance.option + ".dat")
            ? ReturnFlow.None
            : ReturnFlow.SkipOriginal;
    }

    private static void PatchCopySaveData(ILManipulationInfo info)
    {
        ILCursor cursor = new(info.Context);
        FieldInfo coroutineStateField = EnumeratorUtils.GetStateMachineStateField(info.Original!.DeclaringType!);
        FieldInfo coroutineThisField = EnumeratorUtils.GetStateMachineThisField(info.Original!.DeclaringType!);

        cursor.GotoNext(x => x.MatchLdstr(""));
        cursor.GotoPrev(x => x.MatchStfld(coroutineStateField));
        cursor.Index++;
        while (!cursor.Current.MatchCallOrCallvirt<InputIO>(nameof(InputIO.CreateFile)))
            cursor.SetAndAdvance(OpCodes.Nop);
        cursor.SetAndAdvance(OpCodes.Nop);
        cursor.EmitLdarg0();
        cursor.EmitLdfld(coroutineThisField);
        cursor.EmitDelegate(CopySaveData);
    }

    private static bool CopySaveData(StartMenu startMenu)
    {
        int sourceSlot = startMenu.selectedfile;
        int destinationSlot = MainManager.instance.option;

        if (!_instance._saveDataPersistence.SaveSlotExistsInVenusRootLoader(sourceSlot))
            return CopyBaseGameFile(sourceSlot, destinationSlot);

        bool result = _instance._saveDataPersistence.CopySaveSlot(sourceSlot, destinationSlot);
        if (!result)
            return false;

        return !File.Exists($"{BaseGameSavesPrefix}{sourceSlot}.dat") ||
               CopyBaseGameFile(sourceSlot, destinationSlot);
    }

    private static bool CopyBaseGameFile(int sourceSlot, int destinationSlot)
    {
        return InputIO.CreateFile(
            $"{BaseGameSavesPrefix}{destinationSlot}.dat",
            InputIO.ReadFile($"{BaseGameSavesPrefix}{sourceSlot}.dat"));
    }
}