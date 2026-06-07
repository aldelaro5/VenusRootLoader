using UnityEngine;

namespace VenusRootLoader.Api.Leaves.MapEntities.Objects.MovingPlatforms;

public sealed class MovingPlatformAlongLerpMapEntityLeaf : MovingPlatformMapEntityLeaf
{
    internal MovingPlatformAlongLerpMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    public override Vector3 EntityStartingPosition
    {
        get => base.EntityStartingPosition;
        set
        {
            base.EntityStartingPosition = value;
            InternalVectorData[0].Value = value;
        }
    }

    public Vector3 ActivePosition { get => InternalVectorData[1].Value; set => InternalVectorData[1].Value = value; }

    public bool StartMovementFromActivePosition
    {
        get => (int)InternalDialogues[0].Value.x == 1;
        set => InternalDialogues[0].Value.x = value ? 1f : 0f;
    }

    internal override void InitializeFromNew()
    {
        base.InitializeFromNew();
        InternalDialogues.AddRange([new(new(0f, 5f, 0f)), new(new(1f, 0f, 0f)), new(new(0f, 0f, 0f))]);
        InternalVectorData.AddRange([new(Vector3.zero), new(Vector3.up)]);
    }
}