using UnityEngine;

namespace VenusRootLoader.Api.Leaves;

public sealed class AnimIdLeaf : Leaf
{
    internal AnimIdLeaf(int gameId, string namedId, string creatorId) : base(gameId, namedId, creatorId) { }

    internal sealed class AnimIdResourcePreload
    {
        internal string ResourcePath { get; set; } = "";
        internal bool PreloadOnlyDuringBattles { get; set; }
        internal bool IsSprite { get; set; }
    }

    public float ShadowSize { get; set; } = 1.0f;
    public Vector3 StartingScale { get; set; } = Vector3.one;
    public float BleepPitch { get; set; } = 1.0f;
    public Branch<DialogueBleepLeaf> BleepId { get; set; }
    public bool IsModelEntity { get; set; }
    public Vector3 ModelScale { get; set; }
    public Vector3 ModelOffset { get; set; }
    public Vector3 FreezeSize { get; set; }
    public Vector3 FreezeOffset { get; set; }
    public Vector3 FreezeFlipOffset { get; set; }
    internal List<AnimIdResourcePreload> PreloadResources { get; } = new();
    public bool ShakeOnDrop { get; set; }
    public bool HasDigAnimation { get; set; }
    public bool HasJumpAnimationOverride { get; set; } = true;
    public bool FallsWhenFrozen { get; set; } = true;
    public bool HasShadow { get; set; } = true;
    public EntityControl.WalkType WalkType { get; set; }
    internal int UnusedBaseIdleAnimState { get; set; }
    internal int UnusedBaseWalkAnimState { get; set; }
    public float MinimumHeight { get; set; }
    internal float UnusedStartingHeight { get; set; }
    public float StartingBobSpeed { get; set; }
    public float StartingBobFrequency { get; set; }
    public bool HasIceAnimation { get; set; }
    public bool HasFlyingAnimationOverride { get; set; }
    public bool ForcesShadow { get; set; }
    public bool Object { get; set; }
}