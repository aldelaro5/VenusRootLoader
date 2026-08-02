using AssetsTools.NET.Extra;

namespace VenusRootLoader.Bootstrap.Unity.GlobalManagers;

internal interface IGlobalManagersPatcher
{
    bool ShouldPatch(AssetsManager assetsManager, AssetsFileInstance globalManagersFileInstance);
    void Patch(AssetsManager assetsManager, AssetsFileInstance globalManagersFileInstance);
}