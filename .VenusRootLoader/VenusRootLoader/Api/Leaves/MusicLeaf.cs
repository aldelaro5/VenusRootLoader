using UnityEngine;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves;

[ExposeFromVenus]
public sealed class MusicLeaf : Leaf
{
    internal MusicLeaf(int gameId, string creatorId, string namedId) : base(gameId, creatorId, namedId)
    {
    }

    // TODO: Receive as a mandatory parameter to Venus
    public AudioClip Music { get; set; } = null!;
    public float? LoopEndTimestampInSeconds { get; set; }
    public float? LoopStartTimestampInSeconds { get; set; }
    public bool CanBePurchasedFromSamira { get; set; } = true;
    public LocalizedData<string> SamiraDisplayTitle { get; } = new();
}