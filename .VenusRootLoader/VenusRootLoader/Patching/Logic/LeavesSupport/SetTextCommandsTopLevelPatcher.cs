using MonoDetour.Cil;
using MonoMod.Cil;
using System.Collections;
using System.Reflection;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Extensions;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Logic.LeavesSupport;

/// <summary>
/// This patcher allows the <see cref="SetTextCommandLeaf"/> registry to function.
/// <p>
/// It patches the following:
/// <list type="bullet">
/// <item><see cref="MainManager.SetText(string, int, float?, bool, bool, Vector3, Vector3, Vector2, Transform, NPCControl)"/>:
/// Changes the command switch to reflect the delegates from the <see cref="SetTextCommandLeaf"/> registry.</item>
/// </list>
/// </p>
/// </summary>
internal sealed class SetTextCommandsTopLevelPatcher : ITopLevelPatcher
{
    private static SetTextCommandsTopLevelPatcher _instance = null!;

    private readonly ILeavesRegistry<SetTextCommandLeaf> _setTextCommandsRegistry;

    public SetTextCommandsTopLevelPatcher(ILeavesRegistry<SetTextCommandLeaf> setTextCommandsRegistry)
    {
        _instance = this;
        _setTextCommandsRegistry = setTextCommandsRegistry;
    }

    public void Patch()
    {
        Md.MainManager
            .SetText_System_String_System_Int32_System_Nullable_1_System_Boolean_System_Boolean_UnityEngine_Vector3_UnityEngine_Vector3_UnityEngine_Vector2_UnityEngine_Transform_NPCControl
            .ILHookMoveNext(PatchSetTextCommandSwitch);
    }

    private static void PatchSetTextCommandSwitch(ILManipulationInfo info)
    {
        ILCursor cursor = new(info.Context);
        FieldInfo setTextStateMachineCommandContextField = EnumeratorUtils.GetStateMachineFieldFromUniqueType(
            info.Original!.DeclaringType!,
            typeof(MainManager.SetTextCommandContext));

        cursor.GotoNext(x => x.MatchCallOrCallvirt<MainManager>(nameof(MainManager.SetTalk)));
        cursor.GotoNext(x => x.MatchSwitch(out _));
        cursor.Index++;
        ILLabel switchEndLabel = (ILLabel)cursor.Current.Operand;
        int setTextCommandGameIdLocalIndex = 0;
        cursor.GotoPrev(x => x.MatchLdloc(out setTextCommandGameIdLocalIndex));

        cursor.EmitYieldReturnCoroutine(
            info.Original!,
            SetTextHandlerHook,
            ilCursor =>
            {
                ilCursor.EmitLdarg0();
                ilCursor.EmitLdfld(setTextStateMachineCommandContextField);
                ilCursor.EmitLdloc(setTextCommandGameIdLocalIndex);
            },
            switchEndLabel);
    }

    private static IEnumerator SetTextHandlerHook(MainManager.SetTextCommandContext context, int commandGameId)
    {
        return _instance._setTextCommandsRegistry.GetByGameId(commandGameId).Handler?.Invoke(context)
               ?? EnumeratorUtils.EmptyEnumerator;
    }
}