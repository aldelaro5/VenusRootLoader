using Object = UnityEngine.Object;

namespace VenusRootLoader.Patching.Resources;

internal abstract class ResourcesTypePatcher<T> : IResourcesTypePatcher
    where T : Object
{
    public Object PatchResource(string path, Object original) => PatchResource(path, (T)original);
    public abstract T PatchResource(string path, T original);
}