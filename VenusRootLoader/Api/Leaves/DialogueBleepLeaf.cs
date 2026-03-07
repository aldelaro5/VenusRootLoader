using UnityEngine;
using static VenusRootLoader.Unity.SharedAssets;

namespace VenusRootLoader.Api.Leaves;

public sealed class DialogueBleepLeaf : Leaf
{
    public AudioClip BleepSound { get; set; } = CreateDummyAudioClip();
}