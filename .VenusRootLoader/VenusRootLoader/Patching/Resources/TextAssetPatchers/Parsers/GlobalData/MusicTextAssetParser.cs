using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers.GlobalData;

/// <inheritdoc/>
internal sealed class MusicTextAssetParser : ITextAssetParser<MusicLeaf>
{
    public string GetTextAssetSerializedString(string subPath, MusicLeaf leaf)
        => $"{leaf.LoopEndTimestampInSeconds ?? float.MaxValue};{leaf.LoopStartTimestampInSeconds ?? float.MaxValue}";

    public void FromTextAssetSerializedString(string subPath, string text, MusicLeaf leaf)
    {
        string[] fields = text.Split(StringUtils.SemiColonSplitDelimiter);

        float loopEnd = float.Parse(fields[0]);
        float loopStart = float.Parse(fields[1]);
        leaf.LoopEndTimestampInSeconds = Mathf.Approximately(loopEnd, 999f) ? null : loopEnd;
        leaf.LoopStartTimestampInSeconds = Mathf.Approximately(loopStart, 999f) ? null : loopStart;
    }
}