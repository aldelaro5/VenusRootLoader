using HarmonyLib;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using Object = UnityEngine.Object;

namespace VenusRootLoader.Patching.Resources;

/// <summary>
/// A patcher that processes all <see cref="IResourcesTypePatcher{T}"/> and <see cref="IResourcesArrayTypePatcher{T}"/>.
/// Notably, it's where <c>Resources.Load</c> and <c>Resources.LoadAll</c> are patched.
/// The expectation is that each resources type patchers handles patching a specific type of resources.
/// </summary>
internal sealed class ResourcesTopLevelPatcher : ITopLevelPatcher
{
    private static ResourcesTopLevelPatcher _instance = null!;
    private readonly IHarmonyTypePatcher _harmonyTypePatcher;
    private readonly IResourcesTypePatcher<TextAsset> _textAssetPatcher;
    private readonly IResourcesTypePatcher<AudioClip> _audioClipPatcher;
    private readonly IResourcesTypePatcher<Object> _prefabPatcher;
    private readonly IResourcesArrayTypePatcher<Sprite> _spriteArrayPatcher;
    private readonly IResourcesArrayTypePatcher<AudioClip> _audioClipArrayPatcher;

    public ResourcesTopLevelPatcher(
        IHarmonyTypePatcher harmonyTypePatcher,
        IResourcesTypePatcher<TextAsset> textAssetPatcher,
        IResourcesTypePatcher<AudioClip> audioClipPatcher,
        IResourcesTypePatcher<Object> prefabPatcher,
        IResourcesArrayTypePatcher<Sprite> spriteArrayPatcher,
        IResourcesArrayTypePatcher<AudioClip> audioClipArrayPatcher)
    {
        _instance = this;
        _harmonyTypePatcher = harmonyTypePatcher;
        _textAssetPatcher = textAssetPatcher;
        _spriteArrayPatcher = spriteArrayPatcher;
        _prefabPatcher = prefabPatcher;
        _audioClipPatcher = audioClipPatcher;
        _audioClipArrayPatcher = audioClipArrayPatcher;
    }

    public void Patch() => _harmonyTypePatcher.PatchAll(typeof(ResourcesTopLevelPatcher));

    [HarmonyPostfix]
    [HarmonyPatch(typeof(UnityEngine.Resources), nameof(UnityEngine.Resources.Load), [typeof(string), typeof(Type)])]
    private static void PatchResources(string path, Type systemTypeInstance, ref Object __result)
    {
        if (systemTypeInstance == typeof(TextAsset))
            __result = _instance._textAssetPatcher.PatchResource(path, (TextAsset)__result);
        if (systemTypeInstance == typeof(AudioClip))
            __result = _instance._audioClipPatcher.PatchResource(path, (AudioClip)__result);
        if (systemTypeInstance == typeof(Object))
            __result = _instance._prefabPatcher.PatchResource(path, (GameObject)__result);
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

        if (systemTypeInstance == typeof(AudioClip))
        {
            __result = _instance._audioClipArrayPatcher.PatchResources(
                path,
                __result.Cast<AudioClip>().ToArray());
        }
    }
}