using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Api.TextAssetData.Musics;

public sealed class MusicLoopPoints : ITextAssetSerializable
{
    public float EndBoundaryInSeconds { get; set; }
    public float RestartBoundaryInSeconds { get; set; }

    string ITextAssetSerializable.GetTextAssetSerializedString() =>
        $"{EndBoundaryInSeconds};{RestartBoundaryInSeconds}";

    void ITextAssetSerializable.FromTextAssetSerializedString(string text)
    {
        string[] fields = text.Split(StringUtils.SemiColonSplitDelimiter);

        EndBoundaryInSeconds = float.Parse(fields[0]);
        RestartBoundaryInSeconds = float.Parse(fields[1]);
    }
}