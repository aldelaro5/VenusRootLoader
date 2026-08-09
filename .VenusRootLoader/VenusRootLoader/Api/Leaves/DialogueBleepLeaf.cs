using UnityEngine;
using VenusRootLoader.Api.Unity;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves;

[ExposeFromVenus]
public sealed class DialogueBleepLeaf : Leaf
{
    internal DialogueBleepLeaf(int gameId, string creatorId, string namedId) : base(gameId, creatorId, namedId) { }

    public IAssetLoader<AudioClip> BleepSound { get; set; }
}