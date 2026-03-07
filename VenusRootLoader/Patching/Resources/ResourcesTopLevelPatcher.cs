using HarmonyLib;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using Object = UnityEngine.Object;

namespace VenusRootLoader.Patching.Resources;

internal sealed class ResourcesTopLevelPatcher : ITopLevelPatcher
{
    private static ResourcesTopLevelPatcher _instance = null!;
    private readonly IHarmonyTypePatcher _harmonyTypePatcher;
    private readonly IResourcesTypePatcher<UnityEngine.TextAsset> _textAssetPatcher;
    private readonly IResourcesTypePatcher<UnityEngine.AudioClip> _audioClipPatcher;
    private readonly IResourcesArrayTypePatcher<Sprite> _spriteArrayPatcher;
    private readonly IResourcesArrayTypePatcher<UnityEngine.AudioClip> _audioClipArrayPatcher;

    public ResourcesTopLevelPatcher(
        IHarmonyTypePatcher harmonyTypePatcher,
        IResourcesTypePatcher<UnityEngine.TextAsset> textAssetPatcher,
        IResourcesTypePatcher<UnityEngine.AudioClip> audioClipPatcher,
        IResourcesArrayTypePatcher<Sprite> spriteArrayPatcher,
        IResourcesArrayTypePatcher<UnityEngine.AudioClip> audioClipArrayPatcher)
    {
        _instance = this;
        _harmonyTypePatcher = harmonyTypePatcher;
        _textAssetPatcher = textAssetPatcher;
        _spriteArrayPatcher = spriteArrayPatcher;
        _audioClipPatcher = audioClipPatcher;
        _audioClipArrayPatcher = audioClipArrayPatcher;
    }

    public void Patch() => _harmonyTypePatcher.PatchAll(typeof(ResourcesTopLevelPatcher));

    [HarmonyPostfix]
    [HarmonyPatch(typeof(UnityEngine.Resources), nameof(UnityEngine.Resources.Load), [typeof(string), typeof(Type)])]
    private static void PatchResources(string path, Type systemTypeInstance, ref Object __result)
    {
        if (systemTypeInstance == typeof(UnityEngine.TextAsset))
            __result = _instance._textAssetPatcher.PatchResource(path, (UnityEngine.TextAsset)__result);
        if (systemTypeInstance == typeof(UnityEngine.AudioClip))
            __result = _instance._audioClipPatcher.PatchResource(path, (UnityEngine.AudioClip)__result);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(UnityEngine.Resources), nameof(UnityEngine.Resources.LoadAll), [typeof(string), typeof(Type)])]
    [SuppressMessage("ReSharper", "CoVariantArrayConversion")]
    private static void PatchResourcesArray(string path, Type systemTypeInstance, ref Object[] __result)
    {
        if (systemTypeInstance == typeof(Sprite))
        {
            __result = _instance._spriteArrayPatcher.PatchResources(path, __result.Cast<Sprite>().ToArray());
        }

        if (systemTypeInstance == typeof(UnityEngine.AudioClip))
        {
            __result = _instance._audioClipArrayPatcher.PatchResources(
                path,
                __result.Cast<UnityEngine.AudioClip>().ToArray());
        }
    }
}