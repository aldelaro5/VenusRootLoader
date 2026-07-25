using UnityEngine;
using VenusRootLoader.LeavesInternals;

namespace VenusRootLoader.Api.Leaves.MapEntities.Npcs;

public sealed class NpcConditionalEmoticon
{
    internal readonly Ref<Vector2> Vector2Ref = new(new(-1, 0));

    public Branch<FlagLeaf>? RequiredFlag
    {
        get;
        set
        {
            Vector2Ref.Value.x = value?.GameId ?? -1;
            field = value;
        }
    }

    public required NpcEmoticon Emoticon
    {
        get;
        set
        {
            Vector2Ref.Value.y = (int)value;
            field = value;
        }
    }
}