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
    private readonly ILeavesRegistry<MapLeaf> _mapRegistry;
    private readonly ILeavesRegistry<MusicLeaf> _musicRegistry;

    public MapPatcher(string[] subPaths, ILeavesRegistry<MapLeaf> mapRegistry, ILeavesRegistry<MusicLeaf> musicRegistry)
    {
        SubPaths = subPaths;
        _musicRegistry = musicRegistry;
        _mapRegistry = mapRegistry;
    }

    public string[] SubPaths { get; }

    public Object PatchPrefab(string path, Object original)
    {
        int mapEffectiveIdStart = path.LastIndexOf('/') + 1;
        string mapEffectiveId = path[mapEffectiveIdStart..];
        MapLeaf map = _mapRegistry.LeavesByEffectiveIds[mapEffectiveId];
        if (map.PrefabInstantiator is not null)
            return PrepareCustomMap(map);

        GameObject gameObject = (GameObject)original;
        MapControl mapControl = gameObject.GetComponent<MapControl>();

        PatchMusic(mapControl);

        return original;
    }

    private static GameObject PrepareCustomMap(MapLeaf map)
    {
        GameObject prefab = map.PrefabInstantiator!(map);
        prefab.name = map.GameId.ToString();
        MapControl mapControl = prefab.AddComponent<MapControl>();
        mapControl.mapid = (MainManager.Maps)map.GameId;
        mapControl.insidetypes ??= [];
        mapControl.preloadobjs ??= [];
        mapControl.eventPointers ??= [];
        mapControl.entities ??= [];
        mapControl.music ??= [];
        mapControl.dialogues ??= [];
        mapControl.insides ??= [];
        mapControl.entityonly ??= [];
        mapControl.canfollowID ??= [];
        mapControl.discoveryids ??= [];
        mapControl.roundways ??= [];
        mapControl.autoevent ??= [];
        mapControl.mapflags ??= [];
        mapControl.tempfollowers ??= new();
        mapControl.entitysprite ??= new();
        mapControl.musicflags ??= [];
        mapControl.commandlines ??= [];
        Object.Destroy(prefab, 0.001f);
        return prefab;
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