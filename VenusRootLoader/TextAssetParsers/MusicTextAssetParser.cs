using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.TextAssetParsers;

internal sealed class MusicTextAssetParser : ITextAssetParser<MusicLeaf>
{
    public string GetTextAssetSerializedString(string subPath, MusicLeaf leaf)
        => $"{leaf.EndBoundaryInSeconds};{leaf.RestartBoundaryInSeconds}";

    public void FromTextAssetSerializedString(string subPath, string text, MusicLeaf leaf)
    {
        string[] fields = text.Split(StringUtils.SemiColonSplitDelimiter);

        leaf.EndBoundaryInSeconds = float.Parse(fields[0]);
        leaf.RestartBoundaryInSeconds = float.Parse(fields[1]);
    }
}