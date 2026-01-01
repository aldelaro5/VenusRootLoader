using UnityEngine;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Api.TextAssetData.Miscellaneous;

public sealed class BattleTransitionLeaf : ITextAssetSerializable
{
    public Vector2 DestinationPosition { get; set; }

    string ITextAssetSerializable.GetTextAssetSerializedString() => $"{DestinationPosition.x},{DestinationPosition.y}";

    void ITextAssetSerializable.FromTextAssetSerializedString(string text)
    {
        string[] fields = text.Split(StringUtils.CommaSplitDelimiter);

        DestinationPosition = new Vector2(float.Parse(fields[0]), float.Parse(fields[1]));
    }
}