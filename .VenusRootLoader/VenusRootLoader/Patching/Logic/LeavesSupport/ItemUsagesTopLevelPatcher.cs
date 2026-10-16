using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Logic.LeavesSupport;

/// <summary>
/// This patcher allows the <see cref="ItemUsageLeaf"/> registry to function.
/// <p>
/// It patches the following:
/// <list type="bullet">
/// <item><see cref="MainManager.DoItemEffect"/>: Changes the item usage switch to reflect the delegates from the
/// <see cref="ItemUsageLeaf"/> registry.</item>
/// <item><see cref="BattleControl.UseItem"/>: Changes the item usage switch to reflect the delegates from the
/// <see cref="ItemUsageLeaf"/> registry.</item>
/// </list>
/// </p>
/// </summary>
internal sealed class ItemUsagesTopLevelPatcher : ITopLevelPatcher
{
    private static ItemUsagesTopLevelPatcher _instance = null!;

    private readonly IHarmonyTypePatcher _harmonyTypePatcher;
    private readonly ILeavesRegistry<ItemUsageLeaf> _itemUsagesRegistry;

    public ItemUsagesTopLevelPatcher(
        IHarmonyTypePatcher harmonyTypePatcher,
        ILeavesRegistry<ItemUsageLeaf> itemUsagesRegistry)
    {
        _instance = this;
        _harmonyTypePatcher = harmonyTypePatcher;
        _itemUsagesRegistry = itemUsagesRegistry;
    }

    public void Patch() => _harmonyTypePatcher.PatchAll(typeof(ItemUsagesTopLevelPatcher));

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(MainManager), nameof(MainManager.DoItemEffect))]
    private static IEnumerable<CodeInstruction> PatchDoItemEffectSwitch(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator,
        MethodBase method)
    {
        CodeMatcher matcher = new(instructions, generator);

        matcher.MatchStartForward(Code.Switch);
        matcher.Advance(1);
        Label switchEndLabel = (Label)matcher.Operand;
        matcher.MatchStartBackwards(CodeMatch.IsStloc());
        int contextLocalIndex = matcher.Instruction.LocalIndex();
        matcher.Advance(1);
        matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc, contextLocalIndex));
        matcher.Advance(1);
        matcher.InsertAndAdvance(
            Transpilers.EmitDelegate(DoItemEffectHandlerHook),
            new CodeInstruction(OpCodes.Br, switchEndLabel),
            Code.Ldarg_0);

        return matcher.Instructions();
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(BattleControl), nameof(BattleControl.UseItem), MethodType.Enumerator)]
    private static IEnumerable<CodeInstruction> PatchUseItemSwitch(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator,
        MethodBase method)
    {
        CodeMatcher matcher = new(instructions, generator);
        FieldInfo useItemContextField = AccessTools.Field(
            method.DeclaringType,
            StringUtils.ToCamelCase(nameof(BattleControl.UseItemBattleEffectContext)));

        matcher.MatchStartForward(CodeMatch.StoresField(useItemContextField));
        matcher.MatchStartForward(Code.Switch);
        matcher.Advance(1);
        Label switchEndLabel = (Label)matcher.Operand;
        matcher.MatchStartBackwards(CodeMatch.IsLdloc());
        int contextLocalIndex = matcher.Instruction.LocalIndex();
        matcher.InsertAndAdvance(
            new CodeInstruction(OpCodes.Ldloc, 1),
            new CodeInstruction(OpCodes.Ldarg_0),
            new CodeInstruction(OpCodes.Ldfld, useItemContextField));
        matcher.Advance(1);
        matcher.InsertAndAdvance(
            Transpilers.EmitDelegate(UseItemHandlerHook),
            new CodeInstruction(OpCodes.Br, switchEndLabel),
            new CodeInstruction(OpCodes.Ldloc, contextLocalIndex));

        return matcher.Instructions();
    }

    private static void DoItemEffectHandlerHook(MainManager.DoItemEffectContext context, int itemUsageGameId)
    {
        _instance._itemUsagesRegistry.GetByGameId(itemUsageGameId).UsageHandler?.Invoke(context);
    }

    private static void UseItemHandlerHook(
        BattleControl instance,
        BattleControl.UseItemBattleEffectContext context,
        int itemUsageGameId)
    {
        _instance._itemUsagesRegistry.GetByGameId(itemUsageGameId).BattleUsageHandler?.Invoke(instance, context);
    }
}