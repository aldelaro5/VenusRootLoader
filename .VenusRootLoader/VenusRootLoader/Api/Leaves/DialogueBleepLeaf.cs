using UnityEngine;
using VenusRootLoader.SourceGenerators;
using static VenusRootLoader.Unity.SharedAssets;

namespace VenusRootLoader.Api.Leaves;

[ExposeFromVenus]
public sealed class DialogueBleepLeaf : Leaf
{
    internal DialogueBleepLeaf(int gameId, string creatorId, string namedId) : base(gameId, creatorId, namedId) { }

    public AudioClip BleepSound { get; set; } = CreateDummyAudioClip();
}