using UnityEngine;
using VenusRootLoader.Api.Unity;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves;

[ExposeFromVenus]
public sealed class SoundLeaf : Leaf
{
    internal SoundLeaf(int gameId, string creatorId, string namedId) : base(gameId, creatorId, namedId)
    {
    }

    public IAssetLoader<AudioClip> Sound { get; set; } = null!;

    [LeafInitializeFromNew]
    internal void InitializeFromNew(IAssetLoader<AudioClip> sound)
    {
        Sound = sound;
    }
}