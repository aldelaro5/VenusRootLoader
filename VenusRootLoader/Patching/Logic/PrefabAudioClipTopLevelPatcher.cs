using HarmonyLib;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Logic;

internal sealed class PrefabAudioClipTopLevelPatcher : ITopLevelPatcher
{
    private readonly IHarmonyTypePatcher _harmonyTypePatcher;
    private readonly ILeavesRegistry<MusicLeaf> _musicRegistry;

    private static PrefabAudioClipTopLevelPatcher _instance = null!;

    public PrefabAudioClipTopLevelPatcher(
        IHarmonyTypePatcher harmonyTypePatcher,
        ILeavesRegistry<MusicLeaf> musicRegistry)
    {
        _instance = this;
        _harmonyTypePatcher = harmonyTypePatcher;
        _musicRegistry = musicRegistry;
    }

    public void Patch() => _harmonyTypePatcher.PatchAll(typeof(PrefabAudioClipTopLevelPatcher));

    [HarmonyPrefix]
    [HarmonyPatch(typeof(MapControl), nameof(MapControl.Start))]
    internal static bool FixMapPrefabAudioClip(MapControl __instance)
    {
        for (int i = 0; i < __instance.music.Length; i++)
        {
            AudioClip original = __instance.music[i];
            AudioClip newAudioClip = _instance._musicRegistry.LeavesByNamedIds[original.name].Music;
            newAudioClip.name = original.name;
            __instance.music[i] = newAudioClip;
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(FishingMain), nameof(FishingMain.Start))]
    internal static bool FixFishingMainPrefabAudioClip(FishingMain __instance)
    {
        for (int i = 0; i < __instance.musicPreload.Length; i++)
        {
            AudioClip original = __instance.musicPreload[i];
            AudioClip newAudioClip = _instance._musicRegistry.LeavesByNamedIds[original.name].Music;
            newAudioClip.name = original.name;
            __instance.musicPreload[i] = newAudioClip;
        }

        return true;
    }
}