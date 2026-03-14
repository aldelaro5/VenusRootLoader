using Object = UnityEngine.Object;

namespace VenusRootLoader.Patching.Resources.PrefabPatchers;

internal interface IPrefabPatcher
{
    string[] SubPaths { get; }
    Object PatchPrefab(string path, Object original);
}