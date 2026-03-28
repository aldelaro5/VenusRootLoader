using Object = UnityEngine.Object;

namespace VenusRootLoader.Patching.Resources;

/// <summary>
/// A resources patcher that patches the request and return of an array of Unity <see cref="Object"/> assets.
/// </summary>
/// <typeparam name="TObject">The type of Unity <see cref="Object"/> asset this patcher handles</typeparam>
internal interface IResourcesArrayTypePatcher<TObject>
    where TObject : Object
{
    /// <summary>
    /// Processes the patching of the <paramref name="original"/> assets array given that the game requested to load it
    /// from all the ones contained using the <paramref name="path"/> resources path.
    /// </summary>
    /// <param name="path">The resources path the game requested the assets.</param>
    /// <param name="original">The original assets array that would be returned if the patch wasn't present.</param>
    /// <returns>The patched assets array</returns>
    TObject[] PatchResources(string path, TObject[] original);
}