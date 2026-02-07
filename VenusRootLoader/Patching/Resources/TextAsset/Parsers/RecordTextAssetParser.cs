using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Patching.Resources.TextAsset.Parsers;

// TODO: Figure out the ordering, just has "GameId,EnemyPortraitsSpriteIndex" per record
internal sealed class RecordTextAssetParser : ITextAssetParser<RecordLeaf>
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