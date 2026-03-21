using UnityEngine;
using static VenusRootLoader.Unity.SharedAssets;

namespace VenusRootLoader.Api.Leaves;

public sealed class MusicLeaf : Leaf
{
    internal MusicLeaf(int gameId, string namedId, string creatorId) : base(gameId, namedId, creatorId)
    {
    }

    public AudioClip Music { get; set; } = CreateDummyAudioClip();
    public float? LoopEndTimestampInSeconds { get; set; }
    public float? LoopStartTimestampInSeconds { get; set; }
    public bool CanBePurchasedFromSamira { get; set; } = true;
    public LocalizedData<string> SamiraDisplayTitle { get; } = new();
}