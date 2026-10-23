using MonoDetour.Cil;
using MonoMod.Cil;
using System.Reflection;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Extensions;
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

    private readonly ILeavesRegistry<ItemUsageLeaf> _itemUsagesRegistry;

    public ItemUsagesTopLevelPatcher(ILeavesRegistry<ItemUsageLeaf> itemUsagesRegistry)
    {
        _instance = this;
        _itemUsagesRegistry = itemUsagesRegistry;
    }

    public void Patch()
    {
        Md.MainManager.DoItemEffect.ILHook(PatchDoItemEffectSwitch);
        Md.BattleControl.UseItem.ILHookMoveNext(PatchUseItemSwitch);
    }

    private static void PatchDoItemEffectSwitch(ILManipulationInfo info)
    {
        ILCursor cursor = new(info.Context);

        cursor.GotoNext(x => x.MatchSwitch(out _));
        cursor.Index++;
        ILLabel switchEndLabel = (ILLabel)cursor.Current.Operand;
        cursor.GotoPrev(x => x.MatchStfld(out _));
        int contextLocalIndex = 0;
        cursor.GotoNext(x => x.MatchStloc(out contextLocalIndex));
        cursor.Index++;
        cursor.EmitLdloc(contextLocalIndex);
        cursor.Index++;
        cursor.EmitDelegate(DoItemEffectHandlerHook);
        cursor.EmitBr(switchEndLabel);
        cursor.EmitLdarg0();
    }

    private static void PatchUseItemSwitch(ILManipulationInfo info)
    {
        ILCursor cursor = new(info.Context);
        FieldInfo useItemContextField = EnumeratorUtils.GetStateMachineFieldFromUniqueType(
            info.Original!.DeclaringType!,
            typeof(BattleControl.UseItemBattleEffectContext));
        FieldInfo thisField = EnumeratorUtils.GetStateMachineThisField(info.Original!.DeclaringType!);

        cursor.GotoNext(x => x.MatchStfld(useItemContextField));
        cursor.GotoNext(x => x.MatchSwitch(out _));
        cursor.Index++;
        ILLabel switchEndLabel = (ILLabel)cursor.Current.Operand;
        int contextLocalIndex = 0;
        cursor.GotoPrev(x => x.MatchLdloc(out contextLocalIndex));

        cursor.EmitLdarg0();
        cursor.EmitLdfld(thisField);
        cursor.EmitLdarg0();
        cursor.EmitLdfld(useItemContextField);
        cursor.Index++;
        cursor.EmitDelegate(UseItemHandlerHook);
        cursor.EmitBr(switchEndLabel);
        cursor.EmitLdloc(contextLocalIndex);
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