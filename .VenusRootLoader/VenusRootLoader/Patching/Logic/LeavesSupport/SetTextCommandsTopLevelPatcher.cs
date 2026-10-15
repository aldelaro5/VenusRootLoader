using HarmonyLib;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using VenusRootLoader.Api.Extensions;
using VenusRootLoader.Api.Leaves;
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

    private readonly IHarmonyTypePatcher _harmonyTypePatcher;
    private readonly ILeavesRegistry<SetTextCommandLeaf> _setTextCommandsRegistry;

    public SetTextCommandsTopLevelPatcher(
        IHarmonyTypePatcher harmonyTypePatcher,
        ILeavesRegistry<SetTextCommandLeaf> setTextCommandsRegistry)
    {
        _instance = this;
        _harmonyTypePatcher = harmonyTypePatcher;
        _setTextCommandsRegistry = setTextCommandsRegistry;
    }

    public void Patch() => _harmonyTypePatcher.PatchAll(typeof(SetTextCommandsTopLevelPatcher));

    [HarmonyTranspiler]
    [HarmonyPatch(
        nameof(MainManager),
        nameof(MainManager.SetText),
        MethodType.Enumerator,
        [
            typeof(string), typeof(int), typeof(float?), typeof(bool), typeof(bool), typeof(Vector3), typeof(Vector3),
            typeof(Vector2), typeof(Transform), typeof(NPCControl)
        ])]
    private static IEnumerable<CodeInstruction> PatchSetTextCommandSwitch(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator,
        MethodBase method)
    {
        CodeMatcher matcher = new(instructions, generator);
        MethodInfo mainManagerSetTalkMethod = AccessTools.Method(
            typeof(MainManager),
            nameof(MainManager.SetTalk));
        FieldInfo setTextStateMachineCommandContextField = AccessTools.Field(
            method.DeclaringType,
            StringUtils.ToCamelCase(nameof(MainManager.SetTextCommandContext)));

        matcher.MatchStartForward(CodeMatch.Calls(mainManagerSetTalkMethod));
        matcher.MatchStartForward(Code.Switch);
        matcher.Advance(1);
        Label switchEndLabel = (Label)matcher.Operand;
        matcher.MatchStartBackwards(CodeMatch.IsLdloc());
        int setTextCommandGameIdLocalIndex = matcher.Instruction.LocalIndex();

        matcher.InsertAndAdvance(
            Transpilers.EmitYieldReturnCoroutine(
                method,
                SetTextHandlerHook,
                matcher,
                [
                    new(OpCodes.Ldarg_0),
                    new(OpCodes.Ldfld, setTextStateMachineCommandContextField),
                    new(CodeInstruction.LoadLocal(setTextCommandGameIdLocalIndex))
                ],
                switchEndLabel));

        return matcher.Instructions();
    }

    private static IEnumerator SetTextHandlerHook(MainManager.SetTextCommandContext context, int commandGameId)
    {
        return _instance._setTextCommandsRegistry.GetByGameId(commandGameId).Handler?.Invoke(context)
               ?? EnumeratorUtils.EmptyEnumerator;
    }
}