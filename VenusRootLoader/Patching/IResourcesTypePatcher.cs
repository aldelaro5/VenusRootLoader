using Object = UnityEngine.Object;

namespace VenusRootLoader.Patching;

internal interface IResourcesTypePatcher
{
    Object PatchResource(string path, Object original);
}

internal abstract class IResourcesTypePatcher<T> : IResourcesTypePatcher
    where T : Object
{
    public Object PatchResource(string path, Object original) => PatchResource(path, (T)original);
    public abstract T PatchResource(string path, T original);
}