using UnityEngine;

namespace VenusRootLoader.Api.Leaves;

internal sealed class AnimIdLeaf : Leaf
{
    internal sealed class AnimIdResourcePreload
    {
        internal string ResourcePath { get; set; } = "";
        internal bool PreloadOnlyDuringBattles { get; set; }
        internal bool IsSprite { get; set; }
    }

    internal float ShadowSize { get; set; } = 1.0f;
    internal Vector3 StartScale { get; set; } = Vector3.one;
    internal float BleepPitch { get; set; } = 1.0f;
    internal int BleepId { get; set; }
    internal bool IsModelEntity { get; set; }
    internal Vector3 ModelScale { get; set; }
    internal Vector3 ModelOffset { get; set; }
    internal Vector3 FreezeSize { get; set; }
    internal Vector3 FreezeOffset { get; set; }
    internal Vector3 FreezeFlipOffset { get; set; }
    internal List<AnimIdResourcePreload> PreloadResources { get; } = new();
    internal bool ShakeOnDrop { get; set; }
    internal bool HasDigAnimation { get; set; }
    internal bool DoNotOverrideJump { get; set; }
    internal bool DontFreezeWhenFalling { get; set; }
    internal bool HasNoShadows { get; set; }
    internal EntityControl.WalkType WalkType { get; set; }
    internal int UnusedBaseState { get; set; }
    internal int UnusedBaseWalk { get; set; }
    internal float MinimumHeight { get; set; }
    internal float UnusedStartingHeight { get; set; }
    internal float StartingBobSpeed { get; set; }
    internal float StartingBobRange { get; set; }
    internal bool HasIceAnimation { get; set; }
    internal bool HasNoFlyAnimation { get; set; }
    internal bool ForcesShadow { get; set; }
    internal bool Object { get; set; }
}