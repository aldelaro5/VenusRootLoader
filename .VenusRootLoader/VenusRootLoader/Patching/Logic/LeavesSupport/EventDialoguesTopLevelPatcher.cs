using MonoDetour.Cil;
using MonoMod.Cil;
using System.Collections;
using System.Reflection;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Extensions;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Logic.LeavesSupport;

/// <summary>
/// This patcher allows the <see cref="BattleEventDialogueLeaf"/> registry to function.
/// <p>
/// It patches the following:
/// <list type="bullet">
/// <item><see cref="BattleControl.EventDialogue"/>: Changes the main switch to reflect the delegates from the
/// <see cref="BattleEventDialogueLeaf"/> registry.</item>
/// </list>
/// </p>
/// </summary>
internal sealed class EventDialoguesTopLevelPatcher : ITopLevelPatcher
{
    private static EventDialoguesTopLevelPatcher _instance = null!;

    private readonly ILeavesRegistry<BattleEventDialogueLeaf> _battleEventDialoguesRegistry;

    public EventDialoguesTopLevelPatcher(ILeavesRegistry<BattleEventDialogueLeaf> battleEventDialoguesRegistry)
    {
        _instance = this;
        _battleEventDialoguesRegistry = battleEventDialoguesRegistry;
    }

    public void Patch()
    {
        Md.BattleControl.EventDialogue.ILHookMoveNext(PatchEventDialogue);
    }

    private static void PatchEventDialogue(ILManipulationInfo info)
    {
        ILCursor cursor = new(info.Context);
        FieldInfo thisField = EnumeratorUtils.GetStateMachineThisField(info.Original!.DeclaringType!);

        cursor.GotoNext(x => x.MatchSwitch(out _));
        cursor.Index++;
        cursor.GotoNext(x => x.MatchSwitch(out _));
        cursor.Index++;
        ILLabel switchEndLabel = (ILLabel)cursor.Current.Operand;
        int eventDialogueGameIdLocalIndex = 0;
        cursor.GotoPrev(x => x.MatchLdloc(out eventDialogueGameIdLocalIndex));

        cursor.EmitYieldReturnCoroutine(
            info.Original,
            EventDialogueHandlerHook,
            ilCursor =>
            {
                ilCursor.EmitLdarg0();
                ilCursor.EmitLdfld(thisField);
                ilCursor.EmitLdloc(eventDialogueGameIdLocalIndex);
            },
            switchEndLabel);
    }

    private static IEnumerator EventDialogueHandlerHook(BattleControl battle, int eventDialogueGameId)
    {
        return _instance._battleEventDialoguesRegistry.GetByGameId(eventDialogueGameId).Handler(battle);
    }
}