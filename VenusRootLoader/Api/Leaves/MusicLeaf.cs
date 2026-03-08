using UnityEngine;
using static VenusRootLoader.Unity.SharedAssets;

namespace VenusRootLoader.Api.Leaves;

public sealed class MusicLeaf : Leaf
{
    public AudioClip Music { get; set; } = CreateDummyAudioClip();
    public float? LoopEndTimestampInSeconds { get; set; }
    public float? LoopStartTimestampInSeconds { get; set; }
    public bool CanBePurchasedFromSamira { get; set; } = true;
    public LocalizedData<string> SamiraDisplayTitle { get; } = new();
}