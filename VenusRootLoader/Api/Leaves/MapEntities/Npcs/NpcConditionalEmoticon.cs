using UnityEngine;
using VenusRootLoader.LeavesInternals;

namespace VenusRootLoader.Api.Leaves.MapEntities.Npcs;

public sealed class NpcConditionalEmoticon
{
    internal readonly Ref<Vector2> Ref = new(new(-1, 0));

    public Branch<FlagLeaf>? RequiredFlag
    {
        get;
        set
        {
            Ref.Value.x = value?.GameId ?? -1;
            field = value;
        }
    }

    public required NpcEmoticon Emoticon
    {
        get;
        set
        {
            Ref.Value.y = (int)value;
            field = value;
        }
    }
}