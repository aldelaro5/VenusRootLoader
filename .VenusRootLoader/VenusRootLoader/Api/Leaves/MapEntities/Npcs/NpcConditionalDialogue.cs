using UnityEngine;
using VenusRootLoader.LeavesInternals;

namespace VenusRootLoader.Api.Leaves.MapEntities.Npcs;

public sealed class NpcConditionalDialogue
{
    internal readonly Ref<Vector3> Vector3Ref = new(new(0, -1, 0));

    public Branch<FlagLeaf>? RequiredFlag
    {
        get;
        set
        {
            Vector3Ref.Value.x = value?.GameId ?? -1;
            field = value;
        }
    }

    public required Branch<DialogueLeaf> Dialogue
    {
        get;
        set
        {
            Vector3Ref.Value.y = value.GameId;
            field = value;
        }
    }

    public int DefaultIdleAnimstateWhenSelected
    {
        get;
        set
        {
            Vector3Ref.Value.z = value;
            field = value;
        }
    }
}