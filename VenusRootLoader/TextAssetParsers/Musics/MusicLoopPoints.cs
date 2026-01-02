using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.TextAssetParsers.Musics;

internal sealed class MusicLoopPoints : ITextAssetSerializable
{
    internal float EndBoundaryInSeconds { get; set; }
    internal float RestartBoundaryInSeconds { get; set; }

    string ITextAssetSerializable.GetTextAssetSerializedString() =>
        $"{EndBoundaryInSeconds};{RestartBoundaryInSeconds}";

    void ITextAssetSerializable.FromTextAssetSerializedString(string text)
    {
        string[] fields = text.Split(StringUtils.SemiColonSplitDelimiter);

        EndBoundaryInSeconds = float.Parse(fields[0]);
        RestartBoundaryInSeconds = float.Parse(fields[1]);
    }
}