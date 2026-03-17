using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers.GlobalData;

internal sealed class MusicTextAssetParser : ITextAssetParser<MusicLeaf>
{
    public string GetTextAssetSerializedString(string subPath, MusicLeaf value)
        => $"{value.LoopEndTimestampInSeconds ?? float.MaxValue};{value.LoopStartTimestampInSeconds ?? float.MaxValue}";

    public void FromTextAssetSerializedString(string subPath, string text, MusicLeaf value)
    {
        string[] fields = text.Split(StringUtils.SemiColonSplitDelimiter);

        float loopEnd = float.Parse(fields[0]);
        float loopStart = float.Parse(fields[1]);
        value.LoopEndTimestampInSeconds = Mathf.Approximately(loopEnd, 999f) ? null : loopEnd;
        value.LoopStartTimestampInSeconds = Mathf.Approximately(loopStart, 999f) ? null : loopStart;
    }
}