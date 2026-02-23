using Object = UnityEngine.Object;

namespace VenusRootLoader.Patching.Resources;

internal interface IResourcesArrayTypePatcher<T>
    where T : Object
{
    T[] PatchResources(string path, T[] original);
}