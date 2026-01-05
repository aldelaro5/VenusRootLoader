using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAsset;

namespace VenusRootLoader.TextAssetParsers;

// TODO: Figure out the ordering, just has "GameId,EnemyPortraitsSpriteIndex" per record
internal sealed class RecordTextAssetParser : ITextAssetSerializable<RecordLeaf>
{
    public string GetTextAssetSerializedString(string subPath, RecordLeaf leaf)
    {
        throw new NotImplementedException();
    }

    public void FromTextAssetSerializedString(string subPath, string text, RecordLeaf leaf)
    {
        throw new NotImplementedException();
    }
}