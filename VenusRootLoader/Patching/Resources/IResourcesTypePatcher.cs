using Object = UnityEngine.Object;

namespace VenusRootLoader.Patching.Resources;

internal interface IResourcesTypePatcher
{
    Object PatchResource(string path, Object original);
}