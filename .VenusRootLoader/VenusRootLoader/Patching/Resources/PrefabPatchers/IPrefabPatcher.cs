using Object = UnityEngine.Object;

namespace VenusRootLoader.Patching.Resources.PrefabPatchers;

/// <summary>
/// A patcher that handles patching specific prefabs (which are Unity <see cref="Object"/>) individually given that the
/// resources path starts with any strings among a list.
/// </summary>
internal interface IPrefabPatcher
{
    /// <summary>
    /// The list of subpaths that this patcher handles. Any resources path excluding <c>Prefabs/</c> that starts with any
    /// element of this array will be processed by this patcher.
    /// </summary>
    string[] SubPaths { get; }

    /// <summary>
    /// Patches the original prefab given that the game requested to load it using the resources path
    /// <paramref name="path"/> excluding the <c>Prefabs/</c> prefix.
    /// </summary>
    /// <param name="path">The resources path the game requested to load excluding the <c>Prefabs/</c> prefix.</param>
    /// <param name="original">The original prefab that would be returned if the patcher wasn't present.</param>
    /// <returns>The patched prefab asset.</returns>
    Object PatchPrefab(string path, Object original);
}