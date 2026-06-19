using CommunityToolkit.Diagnostics;
using UnityEngine;
using VenusRootLoader.LeavesInternals;
using VenusRootLoader.SourceGenerators;

namespace VenusRootLoader.Api.Leaves.MapEntities.Npcs;

public sealed class VenusHealingNpcMapEntityLeaf : NpcMapEntityLeaf
{
    internal VenusHealingNpcMapEntityLeaf(int gameId, string namedId, string creatorId)
        : base(gameId, namedId, creatorId)
    {
    }

    internal override NPCControl.Interaction Interaction => NPCControl.Interaction.VenusHeal;

    public NpcHornInteraction HornInteraction
    {
        get
        {
            if (Modifiers.HasFlag(MapEntityModifiers.ITHD))
                return NpcHornInteraction.InteractWithHornDashOnly;
            return Modifiers.HasFlag(MapEntityModifiers.ITAH)
                ? NpcHornInteraction.InteractWithAnyHornAttack
                : NpcHornInteraction.None;
        }
        set
        {
            switch (value)
            {
                case NpcHornInteraction.None:
                    Modifiers &= ~MapEntityModifiers.ITAH;
                    Modifiers &= ~MapEntityModifiers.ITHD;
                    break;
                case NpcHornInteraction.InteractWithHornDashOnly:
                    Modifiers &= ~MapEntityModifiers.ITAH;
                    Modifiers |= MapEntityModifiers.ITHD;
                    break;
                case NpcHornInteraction.InteractWithAnyHornAttack:
                    Modifiers |= MapEntityModifiers.ITAH;
                    Modifiers &= ~MapEntityModifiers.ITHD;
                    break;
                default:
                    ThrowHelper.ThrowArgumentOutOfRangeException(nameof(PhysicsBehavior));
                    break;
            }
        }
    }

    [MapEntityInitializeFromNew]
    internal override void InitializeFromNew(Vector3 startingPosition, Branch<AnimIdLeaf>? animId)
    {
        base.InitializeFromNew(startingPosition, animId);
    }
}