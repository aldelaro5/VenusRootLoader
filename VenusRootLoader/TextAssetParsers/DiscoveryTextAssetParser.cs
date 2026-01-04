using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAsset;

namespace VenusRootLoader.TextAssetParsers;

// TODO: Figure out the ordering, just has "GameId,EnemyPortraitsSpriteIndex" per discovery
internal sealed class DiscoveryTextAssetParser : ITextAssetSerializable<DiscoveryLeaf>
{
    public string GetTextAssetSerializedString(string subPath, DiscoveryLeaf leaf)
    {
        throw new NotImplementedException();
    }

    public void FromTextAssetSerializedString(string subPath, string text, DiscoveryLeaf leaf)
    {
        throw new NotImplementedException();
    }
}