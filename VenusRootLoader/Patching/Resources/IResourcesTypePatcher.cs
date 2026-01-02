using Object = UnityEngine.Object;

namespace VenusRootLoader.Patching.Resources;

internal interface IResourcesTypePatcher<T>
    where T : Object
{
    T PatchResource(string path, T original);
}