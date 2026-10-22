using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;

namespace VenusRootLoader.Patching.Logic.LeavesSupport;

/// <summary>
/// This patcher allows the <see cref="ListTypeLeaf"/> registry to function.
/// <p>
/// It patches the following:
/// <list type="bullet">
/// <item><see cref="MainManager.ShowItemList"/>: Changes the setup and render switches to reflect the delegates from the
/// <see cref="ListTypeLeaf"/> registry.</item>
/// </list>
/// </p>
/// </summary>
internal sealed class ListTypesTopLevelPatcher : ITopLevelPatcher
{
    private static ListTypesTopLevelPatcher _instance = null!;

    private readonly IHarmonyTypePatcher _harmonyTypePatcher;
    private readonly ILeavesRegistry<ListTypeLeaf> _listTypesRegistry;

    public ListTypesTopLevelPatcher(
        IHarmonyTypePatcher harmonyTypePatcher,
        ILeavesRegistry<ListTypeLeaf> listTypesRegistry)
    {
        _instance = this;
        _harmonyTypePatcher = harmonyTypePatcher;
        _listTypesRegistry = listTypesRegistry;
    }

    public void Patch() => _harmonyTypePatcher.PatchAll(typeof(ListTypesTopLevelPatcher));

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(MainManager), nameof(MainManager.ShowItemList))]
    private static IEnumerable<CodeInstruction> PatchShowItemList(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator,
        MethodBase method)
    {
        CodeMatcher matcher = new(instructions, generator);

        matcher.MatchStartForward(Code.Switch);
        matcher.Advance(1);
        Label switchSetupEndLabel = (Label)matcher.Operand;
        matcher.MatchStartBackwards(Code.Stfld);
        matcher.MatchStartForward(CodeMatch.IsStloc());
        int contextSetupLocalIndex = matcher.Instruction.LocalIndex();
        matcher.Advance(1);
        matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc, contextSetupLocalIndex));
        matcher.Advance(1);
        matcher.InsertAndAdvance(
            Transpilers.EmitDelegate(ShowItemListSetupHandlerHook),
            new CodeInstruction(OpCodes.Br, switchSetupEndLabel),
            Code.Ldarg_0);

        matcher.Advance(1);
        matcher.MatchStartForward(Code.Switch);
        matcher.Advance(1);

        matcher.MatchStartForward(Code.Switch);
        matcher.Advance(1);
        Label switchRenderEndLabel = (Label)matcher.Operand;
        matcher.MatchStartBackwards(Code.Stfld);
        matcher.MatchStartForward(CodeMatch.IsStloc());
        int contextRenderLocalIndex = matcher.Instruction.LocalIndex();
        matcher.Advance(1);
        matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc, contextRenderLocalIndex));
        matcher.Advance(1);
        matcher.InsertAndAdvance(
            Transpilers.EmitDelegate(ShowItemListRendererHook),
            new CodeInstruction(OpCodes.Br, switchRenderEndLabel),
            Code.Ldarg_0);

        return matcher.Instructions();
    }

    private static void ShowItemListSetupHandlerHook(MainManager.ShowItemListSetupContext context, int listTypeGameId)
    {
        _instance._listTypesRegistry.GetByGameId(listTypeGameId).SetupHandler.Invoke(context);
    }

    private static void ShowItemListRendererHook(MainManager.ShowItemListRenderContext context, int listTypeGameId)
    {
        _instance._listTypesRegistry.GetByGameId(listTypeGameId).Renderer?.Invoke(context);
    }
}