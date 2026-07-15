using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;
using Object = UnityEngine.Object;

namespace VenusRootLoader.Patching.Resources.PrefabPatchers;

/// <summary>
/// An <see cref="IPrefabPatcher"/> that patches maps prefabs from the game.
/// </summary>
internal sealed class MapPatcher : IPrefabPatcher
{
    private readonly ILeavesRegistry<MusicLeaf> _musicRegistry;

    public MapPatcher(string[] subPaths, ILeavesRegistry<MusicLeaf> musicRegistry)
    {
        SubPaths = subPaths;
        _musicRegistry = musicRegistry;
    }

    public string[] SubPaths { get; }

    public Object PatchPrefab(string path, Object original)
    {
        GameObject gameObject = (GameObject)original;
        MapControl map = gameObject.GetComponent<MapControl>();

        PatchMusic(map);

        return original;
    }

    private void PatchMusic(MapControl map)
    {
        for (int i = 0; i < map.music.Length; i++)
        {
            if (map.music[i] == null)
                continue;
            AudioClip originalClip = map.music[i];
            string originalClipName = originalClip.name;
            AudioClip newAudioClip = _musicRegistry.LeavesByEffectiveIds[originalClipName].Music;

            newAudioClip.name = originalClipName;
            map.music[i] = newAudioClip;
        }
    }
}