using UnityEngine;
using static VenusRootLoader.Unity.SharedAssets;

namespace VenusRootLoader.Api.Leaves;

public sealed class DialogueBleepLeaf : Leaf
{
    internal DialogueBleepLeaf(int gameId, string namedId, string creatorId) : base(gameId, namedId, creatorId) { }

    public AudioClip BleepSound { get; set; } = CreateDummyAudioClip();
}