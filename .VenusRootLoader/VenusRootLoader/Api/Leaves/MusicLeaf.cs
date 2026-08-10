using UnityEngine;
using VenusRootLoader.Api.Unity;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves;

[ExposeFromVenus]
public sealed class MusicLeaf : Leaf
{
    internal MusicLeaf(int gameId, string creatorId, string namedId) : base(gameId, creatorId, namedId)
    {
    }

    public IAssetLoader<AudioClip> Music { get; set; } = null!;
    public float? LoopEndTimestampInSeconds { get; set; }
    public float? LoopStartTimestampInSeconds { get; set; }
    public bool CanBePurchasedFromSamira { get; set; } = true;
    public LocalizedData<string> SamiraDisplayTitle { get; } = new();

    [LeafInitializeFromNew]
    internal void InitializeFromNew(IAssetLoader<AudioClip> music)
    {
        Music = music;
    }
}