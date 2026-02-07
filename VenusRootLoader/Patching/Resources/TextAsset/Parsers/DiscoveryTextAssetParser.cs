using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Patching.Resources.TextAsset.Parsers;

// TODO: Figure out the ordering, just has "GameId,EnemyPortraitsSpriteIndex" per discovery
internal sealed class DiscoveryTextAssetParser : ITextAssetParser<DiscoveryLeaf>
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