using Object = UnityEngine.Object;

namespace VenusRootLoader.Patching.Resources;

/// <summary>
/// A resources patcher that patches the request and return of a single Unity <see cref="Object"/> asset.
/// </summary>
/// <typeparam name="TObject">The type of Unity <see cref="Object"/> asset this patcher handles</typeparam>
internal interface IResourcesTypePatcher<TObject>
    where TObject : Object
{
    /// <summary>
    /// Processes the patching of the <paramref name="original"/> asset given that the game requested to load it using the
    /// <paramref name="path"/> resources path.
    /// </summary>
    /// <param name="path">The resources path the game requested the asset.</param>
    /// <param name="original">The original asset that would be returned if the patch wasn't present.</param>
    /// <returns>The patched asset</returns>
    TObject PatchResource(string path, TObject original);
}