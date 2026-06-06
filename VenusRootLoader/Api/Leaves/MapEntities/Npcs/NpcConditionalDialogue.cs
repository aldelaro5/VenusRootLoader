using UnityEngine;
using VenusRootLoader.LeavesInternals;

namespace VenusRootLoader.Api.Leaves.MapEntities.Npcs;

public sealed class NpcConditionalDialogue
{
    internal readonly Ref<Vector3> Ref = new(new(0, -1, 0));

    public Branch<FlagLeaf>? Flag
    {
        get;
        set
        {
            Ref.Value.x = value?.GameId ?? -1;
            field = value;
        }
    }

    public required Branch<DialogueLeaf> Dialogue
    {
        get;
        set
        {
            Ref.Value.y = value.GameId;
            field = value;
        }
    }

    public int DefaultAnimStateWhenSelected
    {
        get;
        set
        {
            Ref.Value.z = value;
            field = value;
        }
    }
}