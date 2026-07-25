using UnityEngine;
using VenusRootLoader.SourceGenerators;
using static VenusRootLoader.Unity.SharedAssets;

namespace VenusRootLoader.Api.Leaves;

[ExposeFromVenus]
public sealed class DialogueBleepLeaf : Leaf
{
    internal DialogueBleepLeaf(int gameId, string namedId, string creatorId) : base(gameId, namedId, creatorId) { }

    public AudioClip BleepSound { get; set; } = CreateDummyAudioClip();
}